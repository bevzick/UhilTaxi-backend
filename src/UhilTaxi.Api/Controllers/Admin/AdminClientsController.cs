using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;
namespace UhilTaxi.Api.Controllers.Admin;
[ApiController, Route("api/v1/admin/clients"), Authorize(Policy = "ActiveAdmin")]
public sealed class AdminClientsController(UserService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClientResponse>>> List(CancellationToken ct) => Ok(await service.ListClientsAsync(ct));
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ClientResponse>> Get(long id, CancellationToken ct) =>
        await service.GetClientAsync(id, ct) is { } c ? Ok(c) : NotFound();
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<ClientResponse>> Status(long id, UpdateClientStatusRequest request, CancellationToken ct) =>
        await service.SetClientBlockedAsync(id, request.IsBlocked, ct) is { } c ? Ok(c) : NotFound();
}
