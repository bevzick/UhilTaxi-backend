using Microsoft.EntityFrameworkCore;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence;
public sealed class UhilTaxiDbContext(DbContextOptions<UhilTaxiDbContext> options) : DbContext(options)
{
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<Promocode> Promocodes => Set<Promocode>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Promocode> Promocodes => Set<Promocode>();
    public DbSet<PromocodeUsage> PromocodeUsages => Set<PromocodeUsage>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(UhilTaxiDbContext).Assembly);
    }
}
