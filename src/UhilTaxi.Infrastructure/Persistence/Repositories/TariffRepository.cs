using Microsoft.EntityFrameworkCore;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Repositories;
public sealed class TariffRepository(UhilTaxiDbContext db) : ITariffRepository
{
    public Task<List<Tariff>> ListAsync(bool activeOnly, CancellationToken ct) =>
        db.Tariffs.AsNoTracking().Where(t => !activeOnly || t.IsActive).OrderBy(t => t.Id).ToListAsync(ct);
    public Task<Tariff?> GetAsync(long id, CancellationToken ct) => db.Tariffs.FirstOrDefaultAsync(t => t.Id == id, ct);
    public Task<bool> NameExistsAsync(string name, long? excludeId, CancellationToken ct) =>
        db.Tariffs.AnyAsync(t => t.Name == name && (!excludeId.HasValue || t.Id != excludeId.Value), ct);
    public async Task AddAsync(Tariff tariff, CancellationToken ct) => await db.Tariffs.AddAsync(tariff, ct);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
