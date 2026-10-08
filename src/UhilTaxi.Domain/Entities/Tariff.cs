using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Domain.Entities;
public sealed class Tariff
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public TariffServiceClass ServiceClass { get; set; }
    public decimal BaseFare { get; set; }
    public decimal RatePerKm { get; set; }
    public decimal RatePerMin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
