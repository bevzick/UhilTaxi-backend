namespace UhilTaxi.Domain.Entities;
public sealed class Maintenance
{
    public long Id { get; set; }
    public long CarId { get; set; }
    public DateTime ServiceDate { get; set; }
    public string Description { get; set; } = "";
    public decimal Cost { get; set; }
    public int MileageAtService { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
