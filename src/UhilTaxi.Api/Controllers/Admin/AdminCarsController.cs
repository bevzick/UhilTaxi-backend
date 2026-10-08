using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/cars")]
[Authorize(Policy = "ActiveAdmin")]
public sealed class AdminCarsController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.Cars(ct));
    [HttpGet("{id:long}")] public async Task<IActionResult> Get(long id,CancellationToken ct)=>Ok(await service.Car(id,ct));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CarInput request,CancellationToken ct)=>StatusCode(201,await service.AddCar(request.ToEntity(),ct,CurrentId));
    [HttpPatch("{id:long}")] public async Task<IActionResult> Update(long id,[FromBody] CarInput request,CancellationToken ct)=>Ok(await service.UpdateCar(id,request.ToEntity(),ct,CurrentId));
    [HttpPatch("{id:long}/status")] public async Task<IActionResult> Status(long id,[FromBody] CarStatusRequest request,CancellationToken ct)=>Ok(await service.SetCarStatus(id,request.Status,ct,CurrentId));
    public sealed record CarStatusRequest(string Status);
    public sealed record CarInput(long ModelId, string LicensePlate, string VinCode, int Year, string Color)
    {
        public Car ToEntity() => new() { ModelId=ModelId, LicensePlate=LicensePlate, VinCode=VinCode, Year=Year, Color=Color };
    }
}
