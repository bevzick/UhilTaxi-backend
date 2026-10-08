using Microsoft.EntityFrameworkCore;
using TaxiPark.Application.Abstractions.Persistence;
using TaxiPark.Domain.Entities;
namespace TaxiPark.Infrastructure.Persistence.Repositories;
public sealed class AuthRepository(TaxiParkDbContext db) : IAuthRepository
{
    public Task<bool> UserExistsAsync(string phone, string? email, CancellationToken ct) =>
        db.Users.AnyAsync(x => x.Phone == phone || (email != null && x.Email == email), ct);
    public Task<bool> EmailExistsAsync(string email, long exceptUserId, CancellationToken ct) =>
        db.Users.AnyAsync(x => x.Email == email && x.Id != exceptUserId, ct);
    public Task<User?> FindByPhoneAsync(string phone, CancellationToken ct) =>
        db.Users.Include(x => x.ClientProfile).FirstOrDefaultAsync(x => x.Phone == phone, ct);
    public Task<User?> FindByIdAsync(long id, CancellationToken ct) =>
        db.Users.Include(x => x.ClientProfile).FirstOrDefaultAsync(x => x.Id == id, ct);
    public void AddUser(User user) => db.Users.Add(user);
    public void AddRefreshToken(RefreshToken token) => db.RefreshTokens.Add(token);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public async Task<T> InTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var result = await action();
        await transaction.CommitAsync(ct);
        return result;
    }
    public async Task<bool> RevokeRefreshTokenAsync(string hash, bool requireValid, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var affected = await db.RefreshTokens
            .Where(x => x.TokenHash == hash && x.RevokedAt == null && (!requireValid || x.ExpiresAt > now))
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), ct);
        return affected == 1;
    }
    public Task<long?> FindRefreshOwnerAsync(string hash, CancellationToken ct) =>
        db.RefreshTokens.Where(x => x.TokenHash == hash).Select(x => (long?)x.UserId).FirstOrDefaultAsync(ct);
    public async Task RevokeUserRefreshTokensAsync(long userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), ct);
    }
}
