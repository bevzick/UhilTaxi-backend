using TaxiPark.Domain.Enums;

namespace TaxiPark.Domain.Entities;

public sealed class ClientProfile
{
    public long UserId { get; set; }
    public DateOnly? BirthDate { get; set; }
    public User User { get; set; } = null!;
}

// Shared contract: another backend developer may extend this profile with business fields.
