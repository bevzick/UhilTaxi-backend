using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Services;

namespace UhilTaxi.Api.Controllers;

[ApiController, Route("api/v1/orders"), Authorize(Roles = "client")]
public sealed class OrdersController(OrderService orders) : AuthenticatedController
{
    [HttpPost("estimate")]
    public async Task<ActionResult<OrderEstimateResponse>> Estimate(CreateOrderRequest request, CancellationToken ct) =>
        Ok(await orders.EstimateAsync(request, ct));

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var order = await orders.CreateAsync(CurrentId, request, ct);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

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

    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult<OrderResponse>> Cancel(long id, CancelOrderRequest request, CancellationToken ct) =>
        Ok(await orders.CancelAsync(id, CurrentId, CurrentRole, request.Reason, ct));
}
