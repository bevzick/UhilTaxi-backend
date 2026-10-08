using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        StorageMapping.Configure(b, "payments");
        b.HasKey(x => x.Id);
        b.HasOne<Trip>().WithMany().HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdempotencyKey).IsUnique();
        b.HasIndex(x => x.TransactionId).IsUnique();
        b.HasIndex(x => new { x.TripId, x.PaymentStatus });
        b.Property(x => x.Amount).HasPrecision(10,2);
    }
}
