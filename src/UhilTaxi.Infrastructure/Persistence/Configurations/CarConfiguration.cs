using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> b)
    {
        StorageMapping.Configure(b, "cars");
        b.HasKey(x => x.Id);
        b.HasOne<CarModel>().WithMany().HasForeignKey(x => x.ModelId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.LicensePlate).IsUnique();
        b.HasIndex(x => x.VinCode).IsUnique();
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.Year).HasColumnType("year");
    }
}
