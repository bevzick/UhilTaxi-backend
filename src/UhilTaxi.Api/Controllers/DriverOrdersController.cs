using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;

namespace UhilTaxi.Api.Controllers;

[ApiController, Route("api/v1/driver/orders"), Authorize(Roles = "driver")]
public sealed class DriverOrdersController(OrderService orders, TripService trips) : AuthenticatedController
{
    [HttpGet("available")]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> Available(CancellationToken ct,
        int page = 1, int limit = 20, string sort = "created_at") =>
        Ok(await orders.ListAsync(CurrentId, CurrentRole, page, limit, null, sort, true, ct));

    [HttpGet("assigned")]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> Assigned(CancellationToken ct,
        int page = 1, int limit = 20, string? status = null, string sort = "-created_at") =>
        Ok(await orders.ListAsync(CurrentId, CurrentRole, page, limit, status, sort, false, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrderResponse>> Get(long id, CancellationToken ct) =>
        Ok(await orders.GetAsync(id, CurrentId, CurrentRole, ct));

    [HttpGet("{id:long}/history")]
    public async Task<ActionResult<List<OrderHistoryResponse>>> History(long id, CancellationToken ct) =>
        Ok(await orders.HistoryAsync(id, CurrentId, CurrentRole, ct));

    [HttpPost("{id:long}/accept")]
    public async Task<ActionResult<OrderResponse>> Accept(long id, CancellationToken ct) =>
        Ok(await orders.AcceptAsync(id, CurrentId, ct));

    [HttpPost("{id:long}/arrived")]
    public async Task<ActionResult<OrderResponse>> Arrived(long id, CancellationToken ct) =>
        Ok(await orders.ArrivedAsync(id, CurrentId, ct));

    [HttpPost("{id:long}/start")]
    public async Task<ActionResult<TripResponse>> Start(long id, StartTripRequest request, CancellationToken ct)
    {
        var trip = await trips.StartAsync(id, CurrentId, request, ct);
        return CreatedAtRoute("GetDriverTrip", new { id = trip.Id }, trip);
    }

    [HttpPost("{id:long}/complete")]
    public async Task<ActionResult<TripResponse>> Complete(long id, CompleteTripRequest request, CancellationToken ct) =>
        Ok(await trips.CompleteAsync(id, CurrentId, request, ct));
}
