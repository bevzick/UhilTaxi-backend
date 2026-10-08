namespace UhilTaxi.Domain.Entities;
public sealed class EnergyLog
{
    public long Id { get; set; }
    public long ShiftId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "liter";
    public decimal TotalCost { get; set; }
    public string StationName { get; set; } = "";
    public int? OdometerKm { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
