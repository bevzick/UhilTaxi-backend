using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class CarModelConfiguration : IEntityTypeConfiguration<CarModel>
{
    public void Configure(EntityTypeBuilder<CarModel> b)
    {
        StorageMapping.Configure(b, "car_models");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.Brand, x.ModelName, x.FuelType }).IsUnique();
        b.Property(x => x.Category).HasMaxLength(20);
        b.Property(x => x.FuelType).HasMaxLength(20);
    }
}
