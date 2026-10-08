namespace UhilTaxi.Application.DTOs.Responses;

public sealed record Pagination(int Page, int Limit, int Total, int Pages);
public sealed record PagedResponse<T>(IReadOnlyList<T> Data, Pagination Pagination)
{
    public static PagedResponse<T> Create(IReadOnlyList<T> data, int total, int page, int limit) =>
        new(data, new(page, limit, total, (int)Math.Ceiling((double)total / limit)));
}

public sealed record OrderEstimateResponse(decimal DistanceKm, int DurationMin,
    decimal BaseAmount, decimal DiscountAmount, decimal EstimatedFare);
