namespace UhilTaxi.Domain.Entities;
public sealed class Car
{
    public long Id { get; set; }
    public long ModelId { get; set; }
    public string LicensePlate { get; set; } = "";
    public string VinCode { get; set; } = "";
    public int Year { get; set; }
    public string Color { get; set; } = "";
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
