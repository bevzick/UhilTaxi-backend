using Microsoft.EntityFrameworkCore;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Repositories;
public sealed class PromocodeRepository(UhilTaxiDbContext db) : IPromocodeRepository
{
    public Task<List<Promocode>> ListAsync(CancellationToken ct) => db.Promocodes.AsNoTracking()
        .OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    public Task<Promocode?> FindAsync(long id, CancellationToken ct) =>
        db.Promocodes.FirstOrDefaultAsync(p => p.Id == id, ct);
    public Task<bool> CodeExistsAsync(string code, long? exceptId, CancellationToken ct) =>
        db.Promocodes.AnyAsync(p => p.Code == code && (!exceptId.HasValue || p.Id != exceptId.Value), ct);
    public async Task<int> UsesCountAsync(long id, CancellationToken ct)
    {
        // The usage table is added by the orders feature. Do not fail administrative CRUD
        // while that separate module has not been deployed yet.
        var exists = await db.Database.SqlQueryRaw<long>(
            "SELECT COUNT(*) AS Value FROM information_schema.tables " +
            "WHERE table_schema = DATABASE() AND table_name = 'promocode_usages'").SingleAsync(ct);
        if (exists == 0) return 0;
        return checked((int)await db.Database.SqlQueryRaw<long>(
            "SELECT COUNT(*) AS Value FROM promocode_usages WHERE promocode_id = {0}", id).SingleAsync(ct));
    }
    public void Add(Promocode promo) => db.Promocodes.Add(promo);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
