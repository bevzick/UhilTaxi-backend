namespace UhilTaxi.Application.DTOs.Responses;
public sealed record ClientResponse(long Id, string FirstName, string LastName,
    string Phone, string? Email, string Status, DateOnly? BirthDate);
