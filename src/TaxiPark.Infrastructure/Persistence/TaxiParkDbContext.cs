using Microsoft.EntityFrameworkCore;
using TaxiPark.Domain.Entities;
namespace TaxiPark.Infrastructure.Persistence;
public sealed class TaxiParkDbContext(DbContextOptions<TaxiParkDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(TaxiParkDbContext).Assembly);
    }
}
