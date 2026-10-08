namespace UhilTaxi.Application.Abstractions.External;

public sealed record RouteEstimate(decimal DistanceKm, int DurationMin);

public interface IMapsService
{
    Task<RouteEstimate> EstimateAsync(decimal pickupLat, decimal pickupLng,
        decimal destinationLat, decimal destinationLng, CancellationToken ct);
}
