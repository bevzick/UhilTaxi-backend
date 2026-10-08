using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers;
[ApiController, Route("api/v1/driver/shifts")]
[Authorize(Roles = "driver")]
public sealed class DriverShiftsController(OperationsService service) : AuthenticatedController
{
    [HttpPost] public async Task<IActionResult> Open([FromBody] ShiftOpenRequest input,CancellationToken ct)=>StatusCode(201,await service.OpenShift(CurrentId,input.CarId,input.StartMileage,ct));
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.DriverShifts(CurrentId,ct));
    [HttpGet("current")] public async Task<IActionResult> Current(CancellationToken ct)=>Ok(await service.CurrentShift(CurrentId,ct));
    [HttpPost("{shiftId:long}/close")] public async Task<IActionResult> Close(long shiftId,[FromBody] ShiftCloseRequest input,CancellationToken ct)=>Ok(await service.CloseShift(shiftId,CurrentId,input.EndMileage,ct));
    public sealed record ShiftOpenRequest(long CarId,int StartMileage);
    public sealed record ShiftCloseRequest(int EndMileage);
}
