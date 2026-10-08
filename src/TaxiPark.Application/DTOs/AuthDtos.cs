using System.ComponentModel.DataAnnotations;

namespace TaxiPark.Application.DTOs;

public sealed record RegisterRequest(
    [Required, StringLength(50, MinimumLength = 1)] string FirstName,
    [Required, StringLength(50, MinimumLength = 1)] string LastName,
    [Required, RegularExpression(@"^\+[1-9]\d{7,14}$")] string Phone,
    [EmailAddress, StringLength(255)] string? Email,
    [Required, MinLength(12), MaxLength(128)] string Password,
    DateOnly? BirthDate);

public sealed record LoginRequest([Required] string Phone, [Required] string Password);
public sealed record UpdateMeRequest([StringLength(50, MinimumLength = 1)] string? FirstName, [StringLength(50, MinimumLength = 1)] string? LastName, [EmailAddress] string? Email);
public sealed record ChangePasswordRequest([Required] string CurrentPassword, [Required, MinLength(12), MaxLength(128)] string NewPassword);
public sealed record UserResponse(long Id, string FirstName, string LastName, string Phone, string? Email, string Role, string Status, DateOnly? BirthDate);
public sealed record AuthResponse(string AccessToken, int ExpiresIn, UserResponse User);
