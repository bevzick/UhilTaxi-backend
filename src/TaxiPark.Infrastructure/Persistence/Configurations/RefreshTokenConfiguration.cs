using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxiPark.Domain.Entities;
namespace TaxiPark.Infrastructure.Persistence.Configurations;
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        
        entity.ToTable("refresh_tokens");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        entity.HasIndex(x => x.TokenHash).IsUnique();
        entity.HasIndex(x => new { x.UserId, x.ExpiresAt });
        entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
