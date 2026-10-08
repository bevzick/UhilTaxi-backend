using Microsoft.AspNetCore.Identity;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Application.Abstractions.Auth;

namespace UhilTaxi.Infrastructure.Auth;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();
    public string Hash(User user, string password) => _hasher.HashPassword(user, password);
    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
