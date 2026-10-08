using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class PromocodeConfiguration : IEntityTypeConfiguration<Promocode>
{
    public void Configure(EntityTypeBuilder<Promocode> b)
    {
        b.ToTable("promocodes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.DiscountValue).HasColumnName("discount_value").HasPrecision(10,2).IsRequired();
        b.Property(x => x.DiscountType).HasColumnName("discount_type")
            .HasConversion(x => x == DiscountType.Fixed ? "fixed" : "percentage", x => Enum.Parse<DiscountType>(x, true))
            .HasMaxLength(20).IsRequired();
        b.Property(x => x.ExpiryDate).HasColumnName("expiry_date").HasColumnType("date");
        b.Property(x => x.MaxUses).HasColumnName("max_uses");
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.Property(x => x.MinOrderAmount).HasColumnName("min_order_amount").HasPrecision(10,2);
        b.Property(x => x.MaxDiscountAmount).HasColumnName("max_discount_amount").HasPrecision(10,2);
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
