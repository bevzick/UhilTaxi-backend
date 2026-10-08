using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/drivers")]
[Authorize(Policy="ActiveAdmin")]
public sealed class AdminDriverHistoryController(OperationsService service) : AuthenticatedController
{
    [HttpGet("{id:long}/shifts")] public async Task<IActionResult> Shifts(long id,CancellationToken ct)=>Ok(await service.DriverShiftsAdmin(id,ct));
    [HttpGet("{id:long}/trips")] public async Task<IActionResult> Trips(long id,CancellationToken ct)=>Ok(await service.DriverTripsAdmin(id,ct));
}
