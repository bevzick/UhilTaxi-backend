using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Constants;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        StorageMapping.Configure(b, "orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion(v => OrderRules.StatusName(v), v => OrderRules.ParseStatus(v));
        b.Property(x => x.PickupAddress).HasMaxLength(255).IsRequired();
        b.Property(x => x.DestinationAddress).HasMaxLength(255).IsRequired();
        b.Property(x => x.PickupLat).HasPrecision(10, 7);
        b.Property(x => x.PickupLng).HasPrecision(10, 7);
        b.Property(x => x.DestinationLat).HasPrecision(10, 7);
        b.Property(x => x.DestinationLng).HasPrecision(10, 7);
        b.Property(x => x.EstimatedDistanceKm).HasPrecision(8, 2);
        b.Property(x => x.EstimatedFare).HasPrecision(10, 2);
        b.Property(x => x.CancellationReason).HasMaxLength(500);
        b.HasOne<ClientProfile>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tariff>().WithMany().HasForeignKey(x => x.TariffId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DriverProfile>().WithMany().HasForeignKey(x => x.AssignedDriverId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Promocode>().WithMany().HasForeignKey(x => x.PromocodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ClientId, x.CreatedAt });
        b.HasIndex(x => new { x.Status, x.CreatedAt });
        b.HasIndex(x => new { x.AssignedDriverId, x.Status });
    }
}
