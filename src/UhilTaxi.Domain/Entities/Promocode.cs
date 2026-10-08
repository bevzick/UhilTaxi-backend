using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Domain.Entities;

public sealed class Promocode
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public decimal DiscountValue { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Fixed;
    public DateOnly ExpiryDate { get; set; }
    public int MaxUses { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
