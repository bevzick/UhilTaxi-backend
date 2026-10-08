using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers;
[ApiController, Route("api/v1/driver/violations")]
[Authorize(Roles="driver")]
public sealed class DriverViolationsController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.Violations(CurrentId,ct));
}
