using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Application.DTOs.Responses;

public sealed record TripResponse(long Id, long OrderId, long ShiftId, DateTime ActualStartTime,
    DateTime? ActualEndTime, decimal? DistanceKm, int? DurationMin, string TariffName,
    decimal BaseFare, decimal RatePerKm, decimal RatePerMin, decimal DiscountAmount, decimal? FinalFare)
{
    public static TripResponse From(Trip t) => new(t.Id, t.OrderId, t.ShiftId,
        t.ActualStartTime, t.ActualEndTime, t.DistanceKm, t.DurationMin,
        t.TariffName, t.BaseFare, t.RatePerKm, t.RatePerMin, t.DiscountAmount, t.FinalFare);
}
