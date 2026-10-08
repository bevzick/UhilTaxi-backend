using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/reports")]
[Authorize(Policy="ActiveAdmin")]
public sealed class AdminReportsController(OperationsService service) : AuthenticatedController
{
    [HttpGet("revenue")] public async Task<IActionResult> Revenue(CancellationToken ct,DateTime? from=null,DateTime? to=null)=>Ok(await service.RevenueReport(from??DateTime.UtcNow.Date.AddDays(-30),to??DateTime.UtcNow.Date.AddDays(1),ct));
    [HttpGet("trips")] public async Task<IActionResult> Trips(CancellationToken ct,DateTime? from=null,DateTime? to=null)=>Ok(await service.TripsReport(from??DateTime.UtcNow.Date.AddDays(-30),to??DateTime.UtcNow.Date.AddDays(1),ct));
    [HttpGet("drivers")] public async Task<IActionResult> Drivers(CancellationToken ct)=>Ok(await service.DriversReport(ct));
    [HttpGet("cars")] public async Task<IActionResult> Cars(CancellationToken ct)=>Ok(await service.CarsReport(ct));
}
