namespace TaxiPark.Application.Common;
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "UhilTaxi";
    public string Audience { get; set; } = "UhilTaxi-frontend";
    public string Key { get; set; } = "";
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 14;
}
