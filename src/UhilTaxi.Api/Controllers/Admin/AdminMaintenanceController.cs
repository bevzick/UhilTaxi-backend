using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin")]
[Authorize(Policy = "ActiveAdmin")]
public sealed class AdminMaintenanceController(OperationsService service) : AuthenticatedController
{
    [HttpGet("cars/{carId:long}/maintenance")] public async Task<IActionResult> List(long carId,CancellationToken ct)=>Ok(await service.Maintenance(carId,ct));
    [HttpPost("cars/{carId:long}/maintenance")] public async Task<IActionResult> Create(long carId,[FromBody] MaintenanceInput request,CancellationToken ct)=>StatusCode(201,await service.AddMaintenance(carId,request.ToEntity(),ct,CurrentId));
    [HttpGet("maintenance/{id:long}")] public async Task<IActionResult> Get(long id,CancellationToken ct)=>Ok(await service.MaintenanceById(id,ct));
    [HttpPatch("maintenance/{id:long}")] public async Task<IActionResult> Update(long id,[FromBody] MaintenanceInput request,CancellationToken ct)=>Ok(await service.UpdateMaintenance(id,request.ToEntity(),ct,CurrentId));
    public sealed record MaintenanceInput(DateTime ServiceDate, string Description, decimal Cost, int MileageAtService)
    {
        public Maintenance ToEntity() => new() { ServiceDate=ServiceDate, Description=Description, Cost=Cost, MileageAtService=MileageAtService };
    }
}
