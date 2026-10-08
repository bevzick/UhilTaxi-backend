using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Api.Controllers;

[Authorize]
public abstract class AuthenticatedController : ControllerBase
{
    protected long CurrentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected UserRole CurrentRole => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!, true);
}
