using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxiPark.Domain.Entities;
namespace TaxiPark.Infrastructure.Persistence.Configurations;
public sealed class DriverProfileConfiguration : IEntityTypeConfiguration<DriverProfile>
{
    public void Configure(EntityTypeBuilder<DriverProfile> entity)
    {
        
        entity.ToTable("driver_profiles");
        entity.HasKey(x => x.UserId);
        entity.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedNever();
        entity.Property(x => x.LicenseNumber).HasColumnName("license_number").HasMaxLength(20).IsRequired();
        entity.HasIndex(x => x.LicenseNumber).IsUnique();
        entity.Property(x => x.HireDate).HasColumnName("hire_date");
        entity.Property(x => x.RatingAverage).HasColumnName("rating_average").HasPrecision(3, 2);
        entity.Property(x => x.RatingCount).HasColumnName("rating_count");

    }
}
