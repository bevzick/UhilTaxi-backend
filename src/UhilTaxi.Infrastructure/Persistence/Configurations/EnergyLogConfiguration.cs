using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class EnergyLogConfiguration : IEntityTypeConfiguration<EnergyLog>
{
    public void Configure(EntityTypeBuilder<EnergyLog> b)
    {
        StorageMapping.Configure(b, "energy_logs");
        b.HasKey(x => x.Id);
        b.HasOne<Shift>().WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ShiftId, x.CreatedAt });
        b.Property(x => x.Quantity).HasPrecision(10,2);
        b.Property(x => x.TotalCost).HasPrecision(10,2);
    }
}
