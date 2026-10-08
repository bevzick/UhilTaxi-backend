using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Application.Abstractions.Persistence;

public interface ITripRepository
{
    Task<Trip?> GetAsync(long id, CancellationToken ct);
    Task<(List<Trip> Data, int Total)> ListAsync(long? clientId, long? driverId, int skip, int take, string sort, CancellationToken ct);
    Task<Trip?> GetByOrderAsync(long orderId, CancellationToken ct);
    Task<Shift?> GetShiftForUpdateAsync(long id, CancellationToken ct);
    Task<string?> CarCategoryAsync(long id, CancellationToken ct);
    void Add(Trip trip);
}
