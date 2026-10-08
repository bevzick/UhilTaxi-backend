using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Domain.Entities;

public sealed class DriverProfile
{
    public long UserId { get; set; }
    public string LicenseNumber { get; set; } = "";
    public DateOnly HireDate { get; set; }
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public User User { get; set; } = null!;
}
