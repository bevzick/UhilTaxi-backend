using TaxiPark.Domain.Entities;
namespace TaxiPark.Application.Abstractions.Auth;
public interface ITokenService { string CreateAccessToken(User user); string NewRefreshToken(); string HashRefreshToken(string raw); }
