namespace UhilTaxi.Domain.Entities;

public sealed class AuditLog
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public long? EntityId { get; set; }
    public string? Metadata { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
