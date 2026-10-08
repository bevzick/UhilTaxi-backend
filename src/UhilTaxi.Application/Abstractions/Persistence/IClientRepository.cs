using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.Abstractions.Persistence;
public interface IClientRepository
{
    Task<List<User>> ListAsync(CancellationToken ct);
    Task<User?> FindAsync(long id, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
