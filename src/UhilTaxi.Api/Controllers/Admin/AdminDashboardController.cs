using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/dashboard")]
[Authorize(Policy = "ActiveAdmin")]
public sealed class AdminDashboardController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct)=>Ok(await service.Dashboard(ct));
}
