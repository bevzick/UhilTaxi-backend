namespace UhilTaxi.Domain.Entities;
public sealed class Payment
{
    public long Id { get; set; }
    public long TripId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UAH";
    public string PaymentMethod { get; set; } = "cash";
    public string PaymentStatus { get; set; } = "pending";
    public string? Provider { get; set; }
    public string? TransactionId { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
