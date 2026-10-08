using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class MaintenanceConfiguration : IEntityTypeConfiguration<Maintenance>
{
    public void Configure(EntityTypeBuilder<Maintenance> b)
    {
        StorageMapping.Configure(b, "maintenance");
        b.HasKey(x => x.Id);
        b.HasOne<Car>().WithMany().HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.CarId, x.ServiceDate });
        b.Property(x => x.Cost).HasPrecision(10,2);
    }
}
