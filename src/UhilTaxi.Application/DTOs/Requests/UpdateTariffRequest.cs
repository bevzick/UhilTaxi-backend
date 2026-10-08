namespace UhilTaxi.Application.DTOs.Requests;
public sealed class UpdateTariffRequest
{
    public string? Name { get; set; }
    public string? ServiceClass { get; set; }
    public decimal? BaseFare { get; set; }
    public decimal? RatePerKm { get; set; }
    public decimal? RatePerMin { get; set; }
}
