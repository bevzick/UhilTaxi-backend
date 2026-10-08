using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Application.Services;
public sealed class UserService(IClientRepository repository)
{
    private static ClientResponse Map(User u) => new(u.Id, u.FirstName, u.LastName, u.Phone,
        u.Email, u.Status.ToString().ToLowerInvariant(), u.ClientProfile?.BirthDate);
    public async Task<List<ClientResponse>> ListClientsAsync(CancellationToken ct) =>
        (await repository.ListAsync(ct)).Select(Map).ToList();
    public async Task<ClientResponse?> GetClientAsync(long id, CancellationToken ct) =>
        await repository.FindAsync(id, ct) is { } u ? Map(u) : null;
    public async Task<ClientResponse?> SetClientBlockedAsync(long id, bool blocked, CancellationToken ct)
    {
        var u = await repository.FindAsync(id, ct);
        if (u is null) return null;
        u.Status = blocked ? UserStatus.Blocked : UserStatus.Active;
        u.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return Map(u);
    }
}
