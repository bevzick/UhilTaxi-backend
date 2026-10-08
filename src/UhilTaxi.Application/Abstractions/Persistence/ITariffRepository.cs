using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.Abstractions.Persistence;
public interface ITariffRepository
{
    Task<List<Tariff>> ListAsync(bool activeOnly, CancellationToken ct);
    Task<Tariff?> GetAsync(long id, CancellationToken ct);
    Task<bool> NameExistsAsync(string name, long? excludeId, CancellationToken ct);
    Task AddAsync(Tariff tariff, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
