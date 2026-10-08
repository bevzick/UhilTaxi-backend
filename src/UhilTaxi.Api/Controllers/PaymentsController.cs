using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers;
[ApiController, Route("api/v1/trips/{tripId:long}/payments")]
[Authorize]
public sealed class PaymentsController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> List(long tripId,CancellationToken ct)=>Ok(await service.TripPayments(tripId,CurrentId,CurrentRole==UhilTaxi.Domain.Enums.UserRole.Admin,ct));
    [HttpPost] public async Task<IActionResult> Create(long tripId,[FromBody] PaymentRequest request,CancellationToken ct)=>StatusCode(201,await service.CreatePayment(tripId,CurrentId,request.PaymentMethod,request.IdempotencyKey,ct));
    public sealed record PaymentRequest(string PaymentMethod,string? IdempotencyKey);
}
