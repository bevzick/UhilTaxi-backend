using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;
using UhilTaxi.Application.Exceptions;

namespace UhilTaxi.Api.Controllers.Admin;

[ApiController, Route("api/v1/admin/orders"), Authorize(Roles = "admin")]
public sealed class AdminOrdersController(OrderService orders, TripService trips) : AuthenticatedController
{
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateAdminOrderRequest request, CancellationToken ct)
    {
        var order = await orders.CreateAsync(request.ClientId, request.Order, ct, CurrentId);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPost("estimate")]
    public async Task<ActionResult<OrderEstimateResponse>> Estimate(CreateOrderRequest request, CancellationToken ct) =>
        Ok(await orders.EstimateAsync(request, ct));

    [HttpGet("available")]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> Available(CancellationToken ct,
        int page = 1, int limit = 20, string sort = "created_at") =>
        Ok(await orders.ListAsync(CurrentId, CurrentRole, page, limit, "pending", sort, false, ct));

    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> List(CancellationToken ct,
        int page = 1, int limit = 20, string? status = null, string sort = "-created_at") =>
        Ok(await orders.ListAsync(CurrentId, CurrentRole, page, limit, status, sort, false, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrderResponse>> Get(long id, CancellationToken ct) =>
        Ok(await orders.GetAsync(id, CurrentId, CurrentRole, ct));

    [HttpGet("{id:long}/history")]
    public async Task<ActionResult<List<OrderHistoryResponse>>> History(long id, CancellationToken ct) =>
        Ok(await orders.HistoryAsync(id, CurrentId, CurrentRole, ct));

    [HttpPost("{id:long}/assign-driver")]
    public async Task<ActionResult<OrderResponse>> Assign(long id, AssignDriverRequest request, CancellationToken ct) =>
        Ok(await orders.AssignAsync(id, CurrentId, request.DriverId, ct));

    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult<OrderResponse>> Cancel(long id, CancelOrderRequest request, CancellationToken ct) =>
        Ok(await orders.CancelAsync(id, CurrentId, CurrentRole, request.Reason, ct));

    [HttpPost("{id:long}/accept")]
    public async Task<ActionResult<OrderResponse>> Accept(long id, AssignDriverRequest request, CancellationToken ct) =>
        Ok(await orders.AcceptAsync(id, request.DriverId, ct, CurrentId));

    [HttpPost("{id:long}/arrived")]
    public async Task<ActionResult<OrderResponse>> Arrived(long id, CancellationToken ct) =>
        Ok(await orders.ArrivedAsync(id, await AssignedDriverAsync(id, ct), ct, CurrentId));

    [HttpPost("{id:long}/start")]
    public async Task<ActionResult<TripResponse>> Start(long id, StartTripRequest request, CancellationToken ct)
    {
        var trip = await trips.StartAsync(id, await AssignedDriverAsync(id, ct), request, ct, CurrentId);
        return CreatedAtAction(nameof(AdminTripsController.Get), "AdminTrips", new { id = trip.Id }, trip);
    }

    [HttpPost("{id:long}/complete")]
    public async Task<ActionResult<TripResponse>> Complete(long id, CompleteTripRequest request, CancellationToken ct) =>
        Ok(await trips.CompleteAsync(id, await AssignedDriverAsync(id, ct), request, ct, CurrentId));

    private async Task<long> AssignedDriverAsync(long id, CancellationToken ct) =>
        (await orders.GetAsync(id, CurrentId, CurrentRole, ct)).AssignedDriverId
        ?? throw new AuthException(409, "ORDER_DRIVER_REQUIRED", "Assign a driver before executing this action.");
}
