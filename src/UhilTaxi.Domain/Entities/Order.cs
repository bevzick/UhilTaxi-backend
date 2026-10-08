using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Domain.Entities;

public sealed class Order
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public long TariffId { get; set; }
    public long? AssignedDriverId { get; set; }
    public long? PromocodeId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string PickupAddress { get; set; } = "";
    public decimal PickupLat { get; set; }
    public decimal PickupLng { get; set; }
    public string DestinationAddress { get; set; } = "";
    public decimal DestinationLat { get; set; }
    public decimal DestinationLng { get; set; }
    public decimal? EstimatedDistanceKm { get; set; }
    public int? EstimatedDurationMin { get; set; }
    public decimal? EstimatedFare { get; set; }
    public string? CancellationReason { get; set; }
    public long? CancelledByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
