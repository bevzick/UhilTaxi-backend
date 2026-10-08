using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Infrastructure.Persistence.Configurations;

public sealed class PromocodeConfiguration : IEntityTypeConfiguration<Promocode>
{
    public void Configure(EntityTypeBuilder<Promocode> b)
    {
        StorageMapping.Configure(b, "promocodes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.DiscountType).HasMaxLength(20).IsRequired();
        b.Property(x => x.DiscountValue).HasPrecision(10, 2);
        b.Property(x => x.MinOrderAmount).HasPrecision(10, 2);
        b.Property(x => x.MaxDiscountAmount).HasPrecision(10, 2);
        b.HasIndex(x => x.Code).IsUnique();
    }
}
