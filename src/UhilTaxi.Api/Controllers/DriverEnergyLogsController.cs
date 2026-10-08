using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers;
[ApiController, Route("api/v1/driver/energy-logs")]
[Authorize(Roles = "driver")]
public sealed class DriverEnergyLogsController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.EnergyLogs(CurrentId,ct));
    [HttpPost] public async Task<IActionResult> Create([FromBody] EnergyLogInput request,CancellationToken ct)=>StatusCode(201,await service.AddEnergy(CurrentId,request.ToEntity(),ct));
    public sealed record EnergyLogInput(decimal Quantity, string Unit, decimal TotalCost, string StationName, int? OdometerKm)
    {
        public EnergyLog ToEntity() => new() { Quantity=Quantity, Unit=Unit, TotalCost=TotalCost, StationName=StationName, OdometerKm=OdometerKm };
    }
}
