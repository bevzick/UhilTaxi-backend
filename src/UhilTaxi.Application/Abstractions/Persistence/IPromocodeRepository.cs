using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.Abstractions.Persistence;
public interface IPromocodeRepository
{
    Task<List<Promocode>> ListAsync(CancellationToken ct);
    Task<Promocode?> FindAsync(long id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, long? exceptId, CancellationToken ct);
    Task<int> UsesCountAsync(long id, CancellationToken ct);
    void Add(Promocode promo);
    Task SaveAsync(CancellationToken ct);
}
