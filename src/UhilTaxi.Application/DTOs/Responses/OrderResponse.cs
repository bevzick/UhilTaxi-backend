using UhilTaxi.Domain.Constants;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Application.DTOs.Responses;

public sealed record OrderResponse(long Id, long ClientId, long TariffId, long? AssignedDriverId,
    string Status, string PickupAddress, decimal PickupLat, decimal PickupLng,
    string DestinationAddress, decimal DestinationLat, decimal DestinationLng,
    decimal? EstimatedDistanceKm, int? EstimatedDurationMin, decimal? EstimatedFare,
    string? CancellationReason, long? CancelledByUserId, DateTime CreatedAt, DateTime UpdatedAt,
    DateTime? AcceptedAt, DateTime? ArrivedAt, DateTime? StartedAt, DateTime? CompletedAt, DateTime? CancelledAt)
{
    public static OrderResponse From(Order o) => new(o.Id, o.ClientId, o.TariffId, o.AssignedDriverId,
        OrderRules.StatusName(o.Status), o.PickupAddress, o.PickupLat, o.PickupLng,
        o.DestinationAddress, o.DestinationLat, o.DestinationLng,
        o.EstimatedDistanceKm, o.EstimatedDurationMin, o.EstimatedFare,
        o.CancellationReason, o.CancelledByUserId, o.CreatedAt, o.UpdatedAt,
        o.AcceptedAt, o.ArrivedAt, o.StartedAt, o.CompletedAt, o.CancelledAt);
}

public sealed record OrderHistoryResponse(long Id, string? FromStatus, string ToStatus,
    long ChangedByUserId, string? Reason, DateTime CreatedAt)
{
    public static OrderHistoryResponse From(OrderStatusHistory h) => new(h.Id,
        h.FromStatus is { } from ? OrderRules.StatusName(from) : null,
        OrderRules.StatusName(h.ToStatus), h.ChangedByUserId, h.Reason, h.CreatedAt);
}
