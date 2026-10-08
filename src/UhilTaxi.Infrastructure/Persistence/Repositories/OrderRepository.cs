using System.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(UhilTaxiDbContext db) : IOrderRepository
{
    public Task<Order?> GetAsync(long id, CancellationToken ct) => db.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Order?> GetForUpdateAsync(long id, CancellationToken ct)
    {
        RequireTransaction();
        // Materialize the parameterized query directly so FOR UPDATE applies to the base table.
        return (await db.Orders.FromSqlInterpolated($"SELECT * FROM orders WHERE id = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
    }

    public async Task<(List<Order> Data, int Total)> ListAsync(OrderQuery query, CancellationToken ct)
    {
        var data = db.Orders.AsNoTracking();
        if (query.ClientId is { } clientId) data = data.Where(x => x.ClientId == clientId);
        if (query.Available && query.DriverId is { } availableDriver)
        {
            var category = await OpenShiftCategoryAsync(availableDriver, ct);
            if (category is null || await HasActiveOrderAsync(availableDriver, ct)) return ([], 0);
            var serviceClass = Enum.Parse<TariffServiceClass>(category, true);
            data = data.Where(x => x.Status == OrderStatus.Pending &&
                (x.AssignedDriverId == null || x.AssignedDriverId == availableDriver) &&
                db.Tariffs.Any(t => t.Id == x.TariffId && t.ServiceClass == serviceClass));
        }
        else if (query.DriverId is { } driverId) data = data.Where(x => x.AssignedDriverId == driverId);
        if (query.Status is { } status) data = data.Where(x => x.Status == status);
        var total = await data.CountAsync(ct);
        var sorted = query.Sort switch
        {
            "id" => data.OrderBy(x => x.Id), "-id" => data.OrderByDescending(x => x.Id),
            "created_at" => data.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            _ => data.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
        };
        return (await sorted.Skip(query.Skip).Take(query.Take).ToListAsync(ct), total);
    }

    public Task<List<OrderStatusHistory>> HistoryAsync(long id, CancellationToken ct) => db.OrderStatusHistory.AsNoTracking()
        .Where(x => x.OrderId == id).OrderBy(x => x.Id).ToListAsync(ct);
    public Task<Tariff?> GetTariffAsync(long id, CancellationToken ct) => db.Tariffs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> ClientExistsAsync(long id, CancellationToken ct) => db.ClientProfiles.AnyAsync(x => x.UserId == id &&
        x.User.Status == UserStatus.Active && x.User.Role == UserRole.Client, ct);

    public async Task<bool> LockDriverAsync(long id, CancellationToken ct)
    {
        RequireTransaction();
        var profiles = await db.DriverProfiles.FromSqlInterpolated(
            $"SELECT * FROM driver_profiles WHERE user_id = {id} FOR UPDATE").ToListAsync(ct);
        return profiles.Count == 1 && await db.Users.AnyAsync(x => x.Id == id &&
            x.Role == UserRole.Driver && x.Status == UserStatus.Active, ct);
    }

    public async Task<string?> OpenShiftCategoryAsync(long driverId, CancellationToken ct)
    {
        var categories = await db.Database.SqlQuery<string>($"""
            SELECT m.category AS Value FROM shifts s
            JOIN cars c ON c.id = s.car_id JOIN car_models m ON m.id = c.model_id
            WHERE s.driver_id = {driverId} AND s.status = 'open' AND s.end_time IS NULL
                AND s.start_time <= UTC_TIMESTAMP() AND c.status = 'active'
            """).ToListAsync(ct);
        return categories.Count == 1 ? categories[0] : null;
    }

    public Task<bool> HasActiveOrderAsync(long driverId, CancellationToken ct) => db.Orders.AnyAsync(x =>
        x.AssignedDriverId == driverId && (x.Status == OrderStatus.Accepted ||
            x.Status == OrderStatus.DriverArriving || x.Status == OrderStatus.InProgress), ct);

    public async Task<Promocode?> GetPromocodeAsync(string code, bool forUpdate, CancellationToken ct)
    {
        if (!forUpdate) return await db.Promocodes.AsNoTracking().SingleOrDefaultAsync(x => x.Code == code, ct);
        RequireTransaction();
        return (await db.Promocodes.FromSqlInterpolated(
            $"SELECT * FROM promocodes WHERE code = {code} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
    }

    public Task<Promocode?> GetPromocodeAsync(long id, CancellationToken ct) =>
        db.Promocodes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<int> PromoUsageCountAsync(long id, CancellationToken ct) => db.PromocodeUsages.CountAsync(x => x.PromocodeId == id, ct);
    public void AddPromoUsage(PromocodeUsage usage) => db.PromocodeUsages.Add(usage);
    public void Add(Order order) => db.Orders.Add(order);
    public void AddHistory(OrderStatusHistory history) => db.OrderStatusHistory.Add(history);
    public Task AuditAsync(long actorId, string action, long orderId, string metadata, DateTime now, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO audit_logs (user_id, action, entity_type, entity_id, metadata, created_at)
            VALUES ({actorId}, {action}, 'order', {orderId}, {metadata}, {now})
            """, ct);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<T> InTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try
        {
            var result = await action();
            await tx.CommitAsync(ct);
            return result;
        }
        catch (Exception ex) when ((ex is MySqlException sql && sql.Number is 1213 or 1205) ||
            (ex is DbUpdateException { InnerException: MySqlException inner } && inner.Number is 1062 or 1213 or 1205))
        {
            throw new AuthException(409, "CONCURRENT_ORDER_UPDATE", "The operation conflicted with another request. Refresh and retry.");
        }
    }

    private void RequireTransaction()
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Row locks require a transaction.");
    }
}
