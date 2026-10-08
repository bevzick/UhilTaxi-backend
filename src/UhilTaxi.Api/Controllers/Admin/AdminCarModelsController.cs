using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/car-models")]
[Authorize(Policy = "ActiveAdmin")]
public sealed class AdminCarModelsController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok(await service.Models(ct));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CarModelInput request,CancellationToken ct)=>StatusCode(201,await service.AddModel(request.ToEntity(),ct,CurrentId));
    [HttpPatch("{id:long}")] public async Task<IActionResult> Update(long id,[FromBody] CarModelInput request,CancellationToken ct)=>Ok(await service.UpdateModel(id,request.ToEntity(),ct,CurrentId));
    public sealed record CarModelInput(string Brand, string ModelName, string Category, string FuelType, short SeatCount)
    {
        public CarModel ToEntity() => new() { Brand=Brand, ModelName=ModelName, Category=Category, FuelType=FuelType, SeatCount=SeatCount };
    }
}
