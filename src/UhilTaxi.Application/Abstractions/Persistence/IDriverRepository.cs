using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.Abstractions.Persistence;
public interface IDriverRepository
{
    Task<List<User>> ListAsync(CancellationToken ct);
    Task<User?> FindAsync(long id, CancellationToken ct);
    Task<bool> UserExistsAsync(string phone, string? email, long? excludingId, CancellationToken ct);
    Task<bool> LicenseExistsAsync(string license, long? excludingId, CancellationToken ct);
    void Add(User user);
    Task SaveAsync(CancellationToken ct);
    Task InTransactionAsync(Func<Task> action, CancellationToken ct);
}
