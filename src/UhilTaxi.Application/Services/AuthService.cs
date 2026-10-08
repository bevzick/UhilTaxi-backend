using UhilTaxi.Application.Abstractions.Auth;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.Common;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Domain.Enums;
using Microsoft.Extensions.Options;
using UhilTaxi.Application.DTOs;

using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Application.Services;

public sealed class AuthService(IAuthRepository db, ITokenService tokens, IPasswordService passwords, IOptions<JwtOptions> options)
{
    private readonly JwtOptions _settings = options.Value;

    private static UserResponse Map(User user) => new(user.Id, user.FirstName, user.LastName,
        user.Phone, user.Email, user.Role.ToString().ToLowerInvariant(),
        user.Status.ToString().ToLowerInvariant(), user.ClientProfile?.BirthDate);
    public UserResponse AsResponse(User user) => Map(user);

    public async Task<(AuthResponse Response, string Refresh)> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();
        if (await db.UserExistsAsync(phone, email, ct))
            throw new AuthException(409, "USER_EXISTS", "Phone or email is already registered.");
        var user = new User { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Phone = phone,
            Email = email, Role = UserRole.Client, Status = UserStatus.Active,
            ClientProfile = new ClientProfile { BirthDate = request.BirthDate } };
        user.PasswordHash = passwords.Hash(user, request.Password);
        db.AddUser(user);
        return await db.InTransactionAsync(async () =>
        {
            await db.SaveChangesAsync(ct);
            return await IssueAsync(user, ct);
        }, ct);
    }

    public async Task<(AuthResponse Response, string Refresh)> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await db.FindByPhoneAsync(request.Phone.Trim(), ct);
        if (user == null || !passwords.Verify(user, request.Password))
            throw new AuthException(401, "INVALID_CREDENTIALS", "Invalid phone or password.");
        CheckActive(user);
        return await IssueAsync(user, ct);
    }

    public async Task<(AuthResponse Response, string Refresh)> RefreshAsync(string? raw, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(raw)) throw new AuthException(401, "MISSING_REFRESH", "Refresh token cookie is missing.");
        var hash = tokens.HashRefreshToken(raw);
        return await db.InTransactionAsync(async () =>
        {
            if (!await db.RevokeRefreshTokenAsync(hash, requireValid: true, ct))
                throw new AuthException(401, "INVALID_REFRESH", "Refresh token is invalid or expired.");
            var owner = await db.FindRefreshOwnerAsync(hash, ct)
                ?? throw new AuthException(401, "INVALID_REFRESH", "Refresh token is invalid.");
            var user = await db.FindByIdAsync(owner, ct)
                ?? throw new AuthException(401, "USER_NOT_FOUND", "User not found.");
            CheckActive(user);
            return await IssueAsync(user, ct);
        }, ct);
    }

    public async Task LogoutAsync(string? raw, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(raw)) return;
        var hash = tokens.HashRefreshToken(raw);
        await db.RevokeRefreshTokenAsync(hash, requireValid: false, ct);
    }

    public async Task<User> GetActiveUserAsync(long id, CancellationToken ct)
    {
        var user = await db.FindByIdAsync(id, ct)
            ?? throw new AuthException(401, "USER_NOT_FOUND", "User not found.");
        CheckActive(user);
        return user;
    }

    public async Task<UserResponse> UpdateMeAsync(long id, UpdateMeRequest data, CancellationToken ct)
    {
        var user = await GetActiveUserAsync(id, ct);
        if (data.FirstName is not null) user.FirstName = data.FirstName.Trim();
        if (data.LastName is not null) user.LastName = data.LastName.Trim();
        if (data.Email is not null)
        {
            var email = string.IsNullOrWhiteSpace(data.Email) ? null : data.Email.Trim().ToLowerInvariant();
            if (email != null && await db.EmailExistsAsync(email, id, ct))
                throw new AuthException(409, "EMAIL_EXISTS", "Email is already registered.");
            user.Email = email;
        }
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(user);
    }

    public async Task ChangePasswordAsync(long id, ChangePasswordRequest data, CancellationToken ct)
    {
        var user = await GetActiveUserAsync(id, ct);
        if (!passwords.Verify(user, data.CurrentPassword))
            throw new AuthException(400, "INVALID_PASSWORD", "Current password is incorrect.");
        user.PasswordHash = passwords.Hash(user, data.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await db.InTransactionAsync(async () =>
        {
            await db.RevokeUserRefreshTokensAsync(id, ct);
            await db.SaveChangesAsync(ct);
            return true;
        }, ct);
    }

    private async Task<(AuthResponse Response, string Refresh)> IssueAsync(User user, CancellationToken ct)
    {
        var refresh = tokens.NewRefreshToken();
        db.AddRefreshToken(new RefreshToken { UserId = user.Id, TokenHash = tokens.HashRefreshToken(refresh),
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshDays) });
        await db.SaveChangesAsync(ct);
        return (new AuthResponse(tokens.CreateAccessToken(user), _settings.AccessMinutes * 60, Map(user)), refresh);
    }

    private static void CheckActive(User user)
    {
        if (user.Status != UserStatus.Active)
            throw new AuthException(403, "USER_BLOCKED", "User account is blocked.");
    }
}
