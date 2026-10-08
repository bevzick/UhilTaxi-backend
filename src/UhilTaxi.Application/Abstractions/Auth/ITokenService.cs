using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.Abstractions.Auth;
public interface ITokenService { string CreateAccessToken(User user); string NewRefreshToken(); string HashRefreshToken(string raw); }
