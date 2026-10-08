using TaxiPark.Domain.Entities;
namespace TaxiPark.Application.Abstractions.Persistence;
public interface IAuthRepository
{
    Task<bool> UserExistsAsync(string phone, string? email, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, long exceptUserId, CancellationToken ct);
    Task<User?> FindByPhoneAsync(string phone, CancellationToken ct);
    Task<User?> FindByIdAsync(long id, CancellationToken ct);
    void AddUser(User user);
    void AddRefreshToken(RefreshToken token);
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task<T> InTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct);
    Task<bool> RevokeRefreshTokenAsync(string hash, bool requireValid, CancellationToken ct);
    Task<long?> FindRefreshOwnerAsync(string hash, CancellationToken ct);
    Task RevokeUserRefreshTokensAsync(long userId, CancellationToken ct);
}
