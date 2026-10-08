using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/drivers"), Authorize(Policy = "ActiveAdmin")]
public sealed class AdminDriversController(DriverService drivers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DriverResponse>>> List(CancellationToken ct) => Ok(await drivers.ListAsync(ct));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<DriverResponse>> Get(long id, CancellationToken ct) =>
        await drivers.GetAsync(id, ct) is { } result ? Ok(result) : NotFound();
    [HttpPost]
    public async Task<ActionResult<DriverResponse>> Create(CreateDriverRequest request, CancellationToken ct)
    {
        var driver = await drivers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = driver.Id }, driver);
    }
    [HttpPatch("{id:long}")]
    public async Task<ActionResult<DriverResponse>> Update(long id, UpdateDriverRequest request, CancellationToken ct) =>
        await drivers.UpdateAsync(id, request, ct) is { } result ? Ok(result) : NotFound();
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<DriverResponse>> Status(long id, UpdateDriverStatusRequest request, CancellationToken ct) =>
        await drivers.SetBlockedAsync(id, request.IsBlocked, ct) is { } result ? Ok(result) : NotFound();
}
