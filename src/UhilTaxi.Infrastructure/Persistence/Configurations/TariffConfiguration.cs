using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class TariffConfiguration : IEntityTypeConfiguration<Tariff>
{
    public void Configure(EntityTypeBuilder<Tariff> b)
    {
        b.ToTable("tariffs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.ServiceClass).HasColumnName("service_class")
            .HasConversion(x => x.ToString().ToLowerInvariant(), x => Enum.Parse<UhilTaxi.Domain.Enums.TariffServiceClass>(x, true))
            .HasMaxLength(20).IsRequired();
        b.Property(x => x.BaseFare).HasColumnName("base_fare").HasPrecision(10,2);
        b.Property(x => x.RatePerKm).HasColumnName("rate_per_km").HasPrecision(10,2);
        b.Property(x => x.RatePerMin).HasColumnName("rate_per_min").HasPrecision(10,2);
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
