namespace UhilTaxi.Domain.Entities;
public sealed class Violation
{
    public long Id { get; set; }
    public long DriverId { get; set; }
    public long CreatedByAdminId { get; set; }
    public string Description { get; set; } = "";
    public decimal FineAmount { get; set; }
    public DateTime ViolationDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
