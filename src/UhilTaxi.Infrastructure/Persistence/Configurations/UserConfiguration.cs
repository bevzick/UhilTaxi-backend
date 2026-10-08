using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Infrastructure.Persistence.Configurations;
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        
        entity.ToTable("users");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).HasColumnName("role");
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
        entity.Property(x => x.FirstName).HasMaxLength(50).HasColumnName("first_name").IsRequired();
        entity.Property(x => x.LastName).HasMaxLength(50).HasColumnName("last_name").IsRequired();
        entity.Property(x => x.Phone).HasMaxLength(20).HasColumnName("phone").IsRequired();
        entity.HasIndex(x => x.Phone).IsUnique();
        entity.Property(x => x.Email).HasMaxLength(255).HasColumnName("email");
        entity.HasIndex(x => x.Email).IsUnique();
        entity.Property(x => x.PasswordHash).HasMaxLength(255).HasColumnName("password_hash").IsRequired();
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        entity.HasOne(x => x.ClientProfile).WithOne(x => x.User).HasForeignKey<ClientProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.DriverProfile).WithOne(x => x.User).HasForeignKey<DriverProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

    }
}
