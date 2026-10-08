using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UhilTaxi.Infrastructure.Persistence;
using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Api.Authorization;

public sealed class ActiveUserRequirement : IAuthorizationRequirement;

public sealed class ActiveUserHandler(UhilTaxiDbContext db) : AuthorizationHandler<ActiveUserRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ActiveUserRequirement requirement)
    {
        if (!long.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return;
        if (await db.Users.AsNoTracking().AnyAsync(x => x.Id == id && x.Status == UserStatus.Active))
            context.Succeed(requirement);
    }
}
