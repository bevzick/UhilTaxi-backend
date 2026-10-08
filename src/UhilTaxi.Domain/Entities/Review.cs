namespace UhilTaxi.Domain.Entities;
public sealed class Review
{
    public long Id { get; set; }
    public long TripId { get; set; }
    public long ReviewerId { get; set; }
    public long TargetUserId { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
