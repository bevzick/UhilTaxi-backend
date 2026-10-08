using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Infrastructure.Persistence.Configurations;

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> b)
    {
        StorageMapping.Configure(b, "trips");
        b.HasKey(x => x.Id);
        b.Property(x => x.TariffName).HasMaxLength(50).IsRequired();
        b.Property(x => x.DistanceKm).HasPrecision(8, 2);
        b.Property(x => x.BaseFare).HasPrecision(10, 2);
        b.Property(x => x.RatePerKm).HasPrecision(10, 2);
        b.Property(x => x.RatePerMin).HasPrecision(10, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(10, 2);
        b.Property(x => x.FinalFare).HasPrecision(10, 2);
        b.HasOne(x => x.Order).WithOne().HasForeignKey<Trip>(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Shift>().WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OrderId).IsUnique();
        b.HasIndex(x => x.ShiftId);
        b.HasIndex(x => x.ActualStartTime);
    }
}
