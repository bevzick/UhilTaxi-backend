using UhilTaxi.Application.Abstractions.Auth;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Application.Services;
public sealed class DriverService(IDriverRepository repository, IPasswordService passwords)
{
    private static DriverResponse Map(User user)
    {
        var profile = user.DriverProfile!;
        return new DriverResponse(user.Id, user.FirstName, user.LastName,
            user.Phone, user.Email, "driver", user.Status.ToString().ToLowerInvariant(),
            profile.LicenseNumber, profile.HireDate, profile.RatingAverage, profile.RatingCount);
    }
    public async Task<List<DriverResponse>> ListAsync(CancellationToken ct) =>
        (await repository.ListAsync(ct)).Select(Map).ToList();
    public async Task<DriverResponse?> GetAsync(long id, CancellationToken ct) =>
        await repository.FindAsync(id, ct) is { } user ? Map(user) : null;
    public async Task<DriverResponse> CreateAsync(CreateDriverRequest request, CancellationToken ct)
    {
        var phone = request.Phone.Trim();
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        var license = request.LicenseNumber.Trim();
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || license.Length == 0)
            throw new AuthException(400, "INVALID_DRIVER", "Name and license number are required.");
        if (request.HireDate == default || request.HireDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new AuthException(400, "INVALID_HIRE_DATE", "Hire date must be valid and not in the future.");
        if (await repository.UserExistsAsync(phone, email, null, ct))
            throw new AuthException(409, "USER_EXISTS", "Phone or email already exists.");
        if (await repository.LicenseExistsAsync(license, null, ct))
            throw new AuthException(409, "LICENSE_EXISTS", "License number already exists.");
        var user = new User
        {
            FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(),
            Phone = phone, Email = email, Role = UserRole.Driver, Status = UserStatus.Active,
            DriverProfile = new DriverProfile { LicenseNumber = license, HireDate = request.HireDate,
                RatingAverage = 0m, RatingCount = 0 }
        };
        user.PasswordHash = passwords.Hash(user, request.Password);
        await repository.InTransactionAsync(async () =>
        {
            repository.Add(user);
            await repository.SaveAsync(ct);
        }, ct);
        return Map(user);
    }
    public async Task<DriverResponse?> UpdateAsync(long id, UpdateDriverRequest request, CancellationToken ct)
    {
        var user = await repository.FindAsync(id, ct);
        if (user == null) return null;
        if (request.FirstName != null)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName)) throw new AuthException(400, "INVALID_NAME", "First name is required.");
            user.FirstName = request.FirstName.Trim();
        }
        if (request.LastName != null)
        {
            if (string.IsNullOrWhiteSpace(request.LastName)) throw new AuthException(400, "INVALID_NAME", "Last name is required.");
            user.LastName = request.LastName.Trim();
        }
        if (request.Email != null)
        {
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
            if (email != null && await repository.UserExistsAsync(user.Phone, email, id, ct))
                throw new AuthException(409, "EMAIL_EXISTS", "Email already exists.");
            user.Email = email;
        }
        if (request.LicenseNumber != null)
        {
            var license = request.LicenseNumber.Trim();
            if (license.Length == 0) throw new AuthException(400, "INVALID_LICENSE", "License number is required.");
            if (await repository.LicenseExistsAsync(license, id, ct))
                throw new AuthException(409, "LICENSE_EXISTS", "License number already exists.");
            user.DriverProfile!.LicenseNumber = license;
        }
        user.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return Map(user);
    }
    public async Task<DriverResponse?> SetBlockedAsync(long id, bool blocked, CancellationToken ct)
    {
        var user = await repository.FindAsync(id, ct);
        if (user == null) return null;
        user.Status = blocked ? UserStatus.Blocked : UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return Map(user);
    }
}
