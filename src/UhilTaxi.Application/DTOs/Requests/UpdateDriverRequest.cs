using System.ComponentModel.DataAnnotations;
namespace UhilTaxi.Application.DTOs.Requests;
public sealed record UpdateDriverRequest(
    [StringLength(50, MinimumLength = 1)] string? FirstName,
    [StringLength(50, MinimumLength = 1)] string? LastName,
    [EmailAddress, StringLength(255)] string? Email,
    [StringLength(20, MinimumLength = 1)] string? LicenseNumber);
