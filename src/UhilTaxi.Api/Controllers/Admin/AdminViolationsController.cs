using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/drivers")]
[Authorize(Policy = "ActiveAdmin")]
public sealed class AdminViolationsController(OperationsService service) : AuthenticatedController
{
    [HttpGet("{id:long}/violations")] public async Task<IActionResult> List(long id,CancellationToken ct)=>Ok(await service.Violations(id,ct));
    [HttpPost("{id:long}/violations")] public async Task<IActionResult> Create(long id,[FromBody] ViolationInput request,CancellationToken ct)=>StatusCode(201,await service.AddViolation(id,CurrentId,request.ToEntity(),ct));
    public sealed record ViolationInput(string Description, decimal FineAmount, DateTime ViolationDate)
    {
        public Violation ToEntity() => new() { Description=Description, FineAmount=FineAmount, ViolationDate=ViolationDate };
    }
}
