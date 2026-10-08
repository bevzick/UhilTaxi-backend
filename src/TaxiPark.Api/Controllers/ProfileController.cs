using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxiPark.Application.Services;
using TaxiPark.Application.Common;
using TaxiPark.Application.DTOs;

namespace TaxiPark.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public sealed class MeController(AuthService auth) : ControllerBase
{
    private long CurrentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<UserResponse>> Get(CancellationToken ct) => Ok(auth.AsResponse(await auth.GetActiveUserAsync(CurrentId, ct)));

    [HttpPatch]
    public async Task<ActionResult<UserResponse>> Update(UpdateMeRequest request, CancellationToken ct) =>
        Ok(await auth.UpdateMeAsync(CurrentId, request, ct));

    [HttpPatch("password")]
    public async Task<IActionResult> Password(ChangePasswordRequest request, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(CurrentId, request, ct);
        return NoContent();
    }
}
