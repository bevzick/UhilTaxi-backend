namespace UhilTaxi.Domain.Entities;

public sealed class PromocodeUsage
{
    public long Id { get; set; }
    public long PromocodeId { get; set; }
    public long ClientId { get; set; }
    public long OrderId { get; set; }
    public DateTime UsedAt { get; set; }
    public Order Order { get; set; } = null!;
}
