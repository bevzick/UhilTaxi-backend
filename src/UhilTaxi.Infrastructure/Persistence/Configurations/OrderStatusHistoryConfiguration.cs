using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UhilTaxi.Domain.Constants;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> b)
    {
        StorageMapping.Configure(b, "order_status_history");
        b.HasKey(x => x.Id);
        b.Property(x => x.FromStatus).HasConversion(v => v.HasValue ? OrderRules.StatusName(v.Value) : null,
            v => v == null ? (OrderStatus?)null : OrderRules.ParseStatus(v));
        b.Property(x => x.ToStatus).HasConversion(v => OrderRules.StatusName(v), v => OrderRules.ParseStatus(v));
        b.Property(x => x.Reason).HasMaxLength(500);
        b.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.OrderId, x.CreatedAt });
    }
}
