using Microsoft.EntityFrameworkCore;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Infrastructure.Persistence.Repositories;
public sealed class DriverRepository(UhilTaxiDbContext db) : IDriverRepository
{
    public Task<List<User>> ListAsync(CancellationToken ct) => db.Users.AsNoTracking()
        .Include(x => x.DriverProfile).Where(x => x.Role == UserRole.Driver)
        .OrderBy(x => x.Id).ToListAsync(ct);
    public Task<User?> FindAsync(long id, CancellationToken ct) => db.Users.Include(x => x.DriverProfile)
        .FirstOrDefaultAsync(x => x.Id == id && x.Role == UserRole.Driver, ct);
    public Task<bool> UserExistsAsync(string phone, string? email, long? excludingId, CancellationToken ct) =>
        db.Users.AnyAsync(x => (!excludingId.HasValue || x.Id != excludingId.Value) &&
            (x.Phone == phone || (email != null && x.Email == email)), ct);
    public Task<bool> LicenseExistsAsync(string license, long? excludingId, CancellationToken ct) =>
        db.DriverProfiles.AnyAsync(x => x.LicenseNumber == license &&
            (!excludingId.HasValue || x.UserId != excludingId.Value), ct);
    public void Add(User user) => db.Users.Add(user);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public async Task InTransactionAsync(Func<Task> action, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await action();
        await tx.CommitAsync(ct);
    }
}
