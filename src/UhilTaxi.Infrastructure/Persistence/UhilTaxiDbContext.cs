using Microsoft.EntityFrameworkCore;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence;
public sealed class UhilTaxiDbContext(DbContextOptions<UhilTaxiDbContext> options) : DbContext(options)
{
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(UhilTaxiDbContext).Assembly);
    }
}
