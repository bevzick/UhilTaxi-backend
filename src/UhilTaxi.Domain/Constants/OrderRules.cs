using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Domain.Constants;

public static class OrderRules
{
    public const decimal MaxFare = 99999999.99m;
    public const decimal MaxDistance = 999999.99m;

    public static string StatusName(OrderStatus status) => status switch
    {
        OrderStatus.DriverArriving => "driver_arriving",
        OrderStatus.InProgress => "in_progress",
        _ => status.ToString().ToLowerInvariant()
    };

    public static OrderStatus ParseStatus(string value) => value switch
    {
        "pending" => OrderStatus.Pending,
        "accepted" => OrderStatus.Accepted,
        "driver_arriving" => OrderStatus.DriverArriving,
        "in_progress" => OrderStatus.InProgress,
        "completed" => OrderStatus.Completed,
        "cancelled" => OrderStatus.Cancelled,
        _ => throw new ArgumentException("Unknown order status.")
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to) => (from, to) switch
    {
        (OrderStatus.Pending, OrderStatus.Accepted) => true,
        (OrderStatus.Accepted, OrderStatus.DriverArriving) => true,
        (OrderStatus.DriverArriving, OrderStatus.InProgress) => true,
        (OrderStatus.InProgress, OrderStatus.Completed) => true,
        (OrderStatus.Pending or OrderStatus.Accepted or OrderStatus.DriverArriving, OrderStatus.Cancelled) => true,
        _ => false
    };

    public static decimal CalculateFare(decimal baseFare, decimal ratePerKm, decimal ratePerMin,
        decimal distanceKm, int durationMin)
    {
        if (baseFare < 0 || ratePerKm < 0 || ratePerMin < 0 || distanceKm < 0 ||
            distanceKm > MaxDistance || durationMin < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm), "Fare inputs must be non-negative and within storage limits.");
        var fare = decimal.Round(baseFare + ratePerKm * distanceKm + ratePerMin * durationMin,
            2, MidpointRounding.AwayFromZero);
        return fare <= MaxFare ? fare : throw new ArgumentOutOfRangeException(nameof(distanceKm), "Fare exceeds the storage limit.");
    }
}
