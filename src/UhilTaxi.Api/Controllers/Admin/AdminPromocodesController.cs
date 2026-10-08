using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/promocodes"), Authorize(Policy = "ActiveAdmin")]
public sealed class AdminPromocodesController(PromocodeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PromocodeResponse>>> List(CancellationToken ct) => Ok(await service.ListAsync(ct));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PromocodeResponse>> Get(long id, CancellationToken ct) =>
        await service.GetAsync(id, ct) is { } p ? Ok(p) : NotFound();
    [HttpPost]
    public async Task<ActionResult<PromocodeResponse>> Create(CreatePromocodeRequest request, CancellationToken ct)
    {
        var p = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }
    [HttpPatch("{id:long}")]
    public async Task<ActionResult<PromocodeResponse>> Update(long id, UpdatePromocodeRequest request, CancellationToken ct) =>
        await service.UpdateAsync(id, request, ct) is { } p ? Ok(p) : NotFound();
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<PromocodeResponse>> Status(long id, UpdatePromocodeStatusRequest request, CancellationToken ct) =>
        await service.SetStatusAsync(id, request.IsActive, ct) is { } p ? Ok(p) : NotFound();
}
