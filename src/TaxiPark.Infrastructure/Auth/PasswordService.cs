using Microsoft.AspNetCore.Identity;
using TaxiPark.Domain.Entities;
using TaxiPark.Application.Abstractions.Auth;

namespace TaxiPark.Infrastructure.Auth;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();
    public string Hash(User user, string password) => _hasher.HashPassword(user, password);
    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
