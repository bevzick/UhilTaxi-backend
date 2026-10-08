using UhilTaxi.Application.Abstractions.External;

namespace UhilTaxi.Infrastructure.External;

// MVP approximation, not road routing: great-circle distance * 1.25, average speed 30 km/h.
public sealed class MockMapsService : IMapsService
{
    public Task<RouteEstimate> EstimateAsync(decimal pickupLat, decimal pickupLng,
        decimal destinationLat, decimal destinationLng, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        const double radians = Math.PI / 180;
        var lat1 = (double)pickupLat * radians;
        var lat2 = (double)destinationLat * radians;
        var deltaLat = lat2 - lat1;
        var deltaLng = (double)(destinationLng - pickupLng) * radians;
        var a = Math.Pow(Math.Sin(deltaLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLng / 2), 2);
        var distance = Math.Max(0.01m, decimal.Round((decimal)(6371 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1))) * 1.25), 2));
        return Task.FromResult(new RouteEstimate(distance, Math.Max(1, (int)Math.Ceiling(distance * 2))));
    }
}
