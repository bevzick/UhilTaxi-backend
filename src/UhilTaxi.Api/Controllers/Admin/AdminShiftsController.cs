using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/shifts")]
[Authorize(Policy = "ActiveAdmin")]
public sealed class AdminShiftsController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.Shifts(ct));
    [HttpGet("{id:long}")] public async Task<IActionResult> Get(long id,CancellationToken ct)=>Ok(await service.Shift(id,ct));
}
