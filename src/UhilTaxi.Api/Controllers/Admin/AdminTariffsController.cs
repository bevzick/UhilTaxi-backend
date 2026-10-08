using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/tariffs"), Authorize(Roles = "admin")]
public sealed class AdminTariffsController(TariffService tariffs) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TariffResponse>>> List(CancellationToken ct) => Ok(await tariffs.ListAsync(false, ct));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<TariffResponse>> Get(long id, CancellationToken ct) =>
        await tariffs.GetAsync(id, ct) is { } t ? Ok(t) : NotFound();
    [HttpPost]
    public async Task<ActionResult<TariffResponse>> Create(CreateTariffRequest request, CancellationToken ct)
    {
        try {
            var created = await tariffs.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        } catch (ArgumentException ex) { return BadRequest(new ProblemDetails { Title = "Invalid tariff", Detail = ex.Message, Status = 400 }); }
          catch (InvalidOperationException) { return Conflict(new ProblemDetails { Title = "Tariff name already exists", Status = 409 }); }
    }
    [HttpPatch("{id:long}")]
    public async Task<ActionResult<TariffResponse>> Update(long id, UpdateTariffRequest request, CancellationToken ct)
    {
        try { return await tariffs.UpdateAsync(id, request, ct) is { } t ? Ok(t) : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new ProblemDetails { Title = "Invalid tariff", Detail = ex.Message, Status = 400 }); }
        catch (InvalidOperationException) { return Conflict(new ProblemDetails { Title = "Tariff name already exists", Status = 409 }); }
    }
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<TariffResponse>> Status(long id, UpdateTariffStatusRequest request, CancellationToken ct) =>
        await tariffs.SetStatusAsync(id, request.IsActive, ct) is { } t ? Ok(t) : NotFound();
}
