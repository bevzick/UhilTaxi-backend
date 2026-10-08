using System.ComponentModel.DataAnnotations;
namespace UhilTaxi.Application.DTOs.Requests;
public sealed class CreateTariffRequest
{
    [Required, StringLength(50, MinimumLength=1)] public string Name { get; set; } = "";
    [Required] public string ServiceClass { get; set; } = "";
    [Range(typeof(decimal), "0", "99999999.99")] public decimal BaseFare { get; set; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal RatePerKm { get; set; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal RatePerMin { get; set; }
}
