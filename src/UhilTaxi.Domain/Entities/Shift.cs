namespace UhilTaxi.Domain.Entities;

// Orders/trips read shifts provisioned by the fleet/shift module.
public sealed class Shift
{
    public long Id { get; set; }
    public long DriverId { get; set; }
    public long CarId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int StartMileage { get; set; }
    public int? EndMileage { get; set; }
    public string Status { get; set; } = "open";
}
