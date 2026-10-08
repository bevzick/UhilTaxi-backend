using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Application.Common;
using UhilTaxi.Application.Abstractions.Auth;

namespace UhilTaxi.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _settings = options.Value;

    public string CreateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToLowerInvariant()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer, audience: _settings.Audience,
            claims: claims, expires: DateTime.UtcNow.AddMinutes(_settings.AccessMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string NewRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public string HashRefreshToken(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
