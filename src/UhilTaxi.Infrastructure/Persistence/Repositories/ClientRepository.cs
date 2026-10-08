using Microsoft.EntityFrameworkCore;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Infrastructure.Persistence.Repositories;
public sealed class ClientRepository(UhilTaxiDbContext db) : IClientRepository
{
    public Task<List<User>> ListAsync(CancellationToken ct) => db.Users.AsNoTracking()
        .Include(u => u.ClientProfile).Where(u => u.Role == UserRole.Client).OrderBy(u => u.Id).ToListAsync(ct);
    public Task<User?> FindAsync(long id, CancellationToken ct) => db.Users.Include(u => u.ClientProfile)
        .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Client, ct);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
