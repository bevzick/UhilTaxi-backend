using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;
namespace UhilTaxi.Api.Controllers;
[ApiController, Route("api/v1/tariffs")]
public sealed class TariffsController(TariffService tariffs) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<List<TariffResponse>>> List(CancellationToken ct) =>
        Ok(await tariffs.ListAsync(true, ct));
}
