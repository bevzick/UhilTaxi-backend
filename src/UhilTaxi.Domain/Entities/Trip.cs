namespace UhilTaxi.Domain.Entities;

public sealed class Trip
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ShiftId { get; set; }
    public DateTime ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public decimal? DistanceKm { get; set; }
    public int? DurationMin { get; set; }
    public string TariffName { get; set; } = "";
    public decimal BaseFare { get; set; }
    public decimal RatePerKm { get; set; }
    public decimal RatePerMin { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal? FinalFare { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Order Order { get; set; } = null!;
}
