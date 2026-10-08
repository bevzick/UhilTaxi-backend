using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Infrastructure.Persistence.Configurations;

public sealed class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> b)
    {
        StorageMapping.Configure(b, "shifts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasOne<DriverProfile>().WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DriverId, x.Status });
        b.HasIndex(x => new { x.CarId, x.Status });
    }
}
