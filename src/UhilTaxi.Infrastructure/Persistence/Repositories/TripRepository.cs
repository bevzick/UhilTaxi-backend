using Microsoft.EntityFrameworkCore;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Infrastructure.Persistence.Repositories;

public sealed class TripRepository(UhilTaxiDbContext db) : ITripRepository
{
    public Task<Trip?> GetAsync(long id, CancellationToken ct) => db.Trips.AsNoTracking().Include(x => x.Order)
        .SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Trip?> GetByOrderAsync(long orderId, CancellationToken ct) => db.Trips.SingleOrDefaultAsync(x => x.OrderId == orderId, ct);

    public async Task<(List<Trip> Data, int Total)> ListAsync(long? clientId, long? driverId,
        int skip, int take, string sort, CancellationToken ct)
    {
        var query = db.Trips.AsNoTracking();
        if (clientId is { } client) query = query.Where(x => x.Order.ClientId == client);
        if (driverId is { } driver) query = query.Where(x => x.Order.AssignedDriverId == driver);
        var total = await query.CountAsync(ct);
        var sorted = sort switch
        {
            "id" => query.OrderBy(x => x.Id), "-id" => query.OrderByDescending(x => x.Id),
            "created_at" => query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            _ => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
        };
        return (await sorted.Skip(skip).Take(take).ToListAsync(ct), total);
    }

    public async Task<Shift?> GetShiftForUpdateAsync(long id, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Row locks require a transaction.");
        return (await db.Shifts.FromSqlInterpolated($"SELECT * FROM shifts WHERE id = {id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
    }

    public async Task<string?> CarCategoryAsync(long id, CancellationToken ct) =>
        (await db.Database.SqlQuery<string>($"""
            SELECT m.category AS Value FROM cars c JOIN car_models m ON m.id = c.model_id
            WHERE c.id = {id} AND c.status = 'active'
            """).ToListAsync(ct)).SingleOrDefault();
    public void Add(Trip trip) => db.Trips.Add(trip);
}
