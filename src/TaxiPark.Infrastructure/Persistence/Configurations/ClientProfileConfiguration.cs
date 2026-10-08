using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxiPark.Domain.Entities;
namespace TaxiPark.Infrastructure.Persistence.Configurations;
public sealed class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
{
    public void Configure(EntityTypeBuilder<ClientProfile> entity)
    {
        
        entity.ToTable("client_profiles");
        entity.HasKey(x => x.UserId);
        entity.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedNever();
        entity.Property(x => x.BirthDate).HasColumnName("birth_date");

    }
}
