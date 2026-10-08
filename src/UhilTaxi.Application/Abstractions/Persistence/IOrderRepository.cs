using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetAsync(long id, CancellationToken ct);
    Task<Order?> GetForUpdateAsync(long id, CancellationToken ct);
    Task<(List<Order> Data, int Total)> ListAsync(OrderQuery query, CancellationToken ct);
    Task<List<OrderStatusHistory>> HistoryAsync(long id, CancellationToken ct);
    Task<Tariff?> GetTariffAsync(long id, CancellationToken ct);
    Task<bool> ClientExistsAsync(long id, CancellationToken ct);
    Task<bool> LockDriverAsync(long id, CancellationToken ct);
    Task<string?> OpenShiftCategoryAsync(long driverId, CancellationToken ct);
    Task<bool> HasActiveOrderAsync(long driverId, CancellationToken ct);
    void Add(Order order);
    void AddHistory(OrderStatusHistory history);
    Task<Promocode?> GetPromocodeAsync(string code, bool forUpdate, CancellationToken ct);
    Task<Promocode?> GetPromocodeAsync(long id, CancellationToken ct);
    Task<int> PromoUsageCountAsync(long id, CancellationToken ct);
    void AddPromoUsage(PromocodeUsage usage);
    Task AuditAsync(long actorId, string action, long orderId, string metadata, DateTime now, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task<T> InTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct);
}

public sealed record OrderQuery(long? ClientId, long? DriverId, OrderStatus? Status,
    bool Available, int Skip, int Take, string Sort);
