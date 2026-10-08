namespace UhilTaxi.Domain.Entities;
public sealed class CarModel
{
    public long Id { get; set; }
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string Category { get; set; } = "economy";
    public string FuelType { get; set; } = "petrol";
    public short SeatCount { get; set; } = 4;
}
