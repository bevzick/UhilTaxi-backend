namespace UhilTaxi.Application.DTOs.Responses;
public sealed record DriverResponse(long Id, string FirstName, string LastName,
    string Phone, string? Email, string Role, string Status,
    string LicenseNumber, DateOnly HireDate, decimal RatingAverage, int RatingCount);
