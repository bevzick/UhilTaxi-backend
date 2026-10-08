using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Domain.Constants;

namespace UhilTaxi.Application.Validation;

public static class OrderValidator
{
    public static void Validate(CreateOrderRequest r)
    {
        if (r.TariffId < 1) Invalid("A valid tariff is required.");
        if (r.Pickup is null || r.Destination is null) Invalid("Pickup and destination are required.");
        Address(r.Pickup!.Address);
        Address(r.Destination!.Address);
        Coordinate(r.Pickup.Lat, 90);
        Coordinate(r.Destination.Lat, 90);
        Coordinate(r.Pickup.Lng, 180);
        Coordinate(r.Destination.Lng, 180);
        if (r.Pickup.Lat == r.Destination.Lat && r.Pickup.Lng == r.Destination.Lng)
            Invalid("Pickup and destination must be different.");
        if (r.Promocode is not null && (string.IsNullOrWhiteSpace(r.Promocode) || r.Promocode.Trim().Length > 20))
            Invalid("Promocode must contain 1-20 characters.");
    }

    public static void Distance(decimal distance)
    {
        if (distance < 0 || distance > OrderRules.MaxDistance || decimal.Round(distance, 2) != distance)
            Invalid("Distance must be non-negative, at most 999999.99 km, and have at most 2 decimal places.");
    }

    public static int Offset(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100 || (long)(page - 1) * pageSize > int.MaxValue)
            Invalid("Page must be positive and page size must be between 1 and 100.");
        return (page - 1) * pageSize;
    }

    private static void Address(string? address)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Trim().Length > 255)
            Invalid("Addresses must contain 1-255 characters.");
    }

    private static void Coordinate(decimal? coordinate, decimal limit)
    {
        if (coordinate is null || coordinate < -limit || coordinate > limit || decimal.Round(coordinate.Value, 7) != coordinate)
            Invalid("Coordinates are required and must be valid latitude/longitude with at most 7 decimal places.");
    }

    private static void Invalid(string message) => throw new AuthException(400, "INVALID_ORDER", message);
}
