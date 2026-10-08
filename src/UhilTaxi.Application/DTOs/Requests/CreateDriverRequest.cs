using System.ComponentModel.DataAnnotations;
namespace UhilTaxi.Application.DTOs.Requests;
public sealed record CreateDriverRequest(
    [Required, StringLength(50, MinimumLength = 1)] string FirstName,
    [Required, StringLength(50, MinimumLength = 1)] string LastName,
    [Required, RegularExpression(@"^\+[1-9]\d{7,14}$")] string Phone,
    [EmailAddress, StringLength(255)] string? Email,
    [Required, MinLength(12), MaxLength(128)] string Password,
    [Required, StringLength(20, MinimumLength = 1)] string LicenseNumber,
    DateOnly HireDate);
