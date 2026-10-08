using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaxiPark.Application.Services;
using TaxiPark.Application.Common;
using TaxiPark.Application.DTOs;

namespace TaxiPark.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(AuthService auth, IOptions<JwtOptions> settings) : ControllerBase
{
    private const string RefreshCookie = "uhiltaxi_refresh";
    private void SetCookie(string refresh) => Response.Cookies.Append(RefreshCookie, refresh, new CookieOptions
    {
        HttpOnly = true, Secure = !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
        SameSite = SameSiteMode.Strict, Path = "/api/v1/auth", IsEssential = true,
        Expires = DateTimeOffset.UtcNow.AddDays(settings.Value.RefreshDays)
    });
    private void RemoveCookie() => Response.Cookies.Delete(RefreshCookie, new CookieOptions { Path = "/api/v1/auth" });

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var (response, refresh) = await auth.RegisterAsync(request, ct);
        SetCookie(refresh);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var (response, refresh) = await auth.LoginAsync(request, ct);
        SetCookie(refresh);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        var (response, refresh) = await auth.RefreshAsync(Request.Cookies[RefreshCookie], ct);
        SetCookie(refresh);
        return Ok(response);
    }

    // Logout revokes the current refresh token; existing access tokens remain valid until expiry.
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await auth.LogoutAsync(Request.Cookies[RefreshCookie], ct);
        RemoveCookie();
        return NoContent();
    }
}
