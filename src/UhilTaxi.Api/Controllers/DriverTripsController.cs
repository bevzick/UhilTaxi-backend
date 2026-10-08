using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;

namespace UhilTaxi.Api.Controllers;

[ApiController, Route("api/v1/driver/trips"), Authorize(Roles = "driver")]
public sealed class DriverTripsController(TripService trips) : AuthenticatedController
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<TripResponse>>> List(CancellationToken ct,
        int page = 1, int limit = 20, string sort = "-created_at") =>
        Ok(await trips.ListAsync(CurrentId, CurrentRole, page, limit, sort, ct));

    [HttpGet("{id:long}", Name = "GetDriverTrip")]
    public async Task<ActionResult<TripResponse>> Get(long id, CancellationToken ct) =>
        Ok(await trips.GetAsync(id, CurrentId, CurrentRole, ct));
}
