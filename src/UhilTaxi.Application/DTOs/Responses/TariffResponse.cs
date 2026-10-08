using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.DTOs.Responses;
public sealed record TariffResponse(long Id, string Name, string ServiceClass, decimal BaseFare,
    decimal RatePerKm, decimal RatePerMin, bool IsActive)
{
    public static TariffResponse From(Tariff t) => new(t.Id, t.Name,
        t.ServiceClass.ToString().ToLowerInvariant(), t.BaseFare, t.RatePerKm, t.RatePerMin, t.IsActive);
}
