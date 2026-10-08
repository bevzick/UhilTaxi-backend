namespace UhilTaxi.Domain.Entities;

public sealed class Promocode
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public decimal DiscountValue { get; set; }
    public string DiscountType { get; set; } = "fixed";
    public DateOnly ExpiryDate { get; set; }
    public int MaxUses { get; set; }
    public bool IsActive { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
