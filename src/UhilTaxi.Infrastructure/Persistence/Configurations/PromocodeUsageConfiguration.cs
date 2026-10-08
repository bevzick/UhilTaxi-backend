using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Infrastructure.Persistence.Configurations;

public sealed class PromocodeUsageConfiguration : IEntityTypeConfiguration<PromocodeUsage>
{
    public void Configure(EntityTypeBuilder<PromocodeUsage> b)
    {
        StorageMapping.Configure(b, "promocode_usages");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.OrderId).IsUnique();
        b.HasOne(x => x.Order).WithOne().HasForeignKey<PromocodeUsage>(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Promocode>().WithMany().HasForeignKey(x => x.PromocodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ClientProfile>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
    }
}
