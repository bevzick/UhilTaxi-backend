using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Infrastructure.Operations;
namespace UhilTaxi.Api.Controllers;
[ApiController, Route("api/v1/trips/{tripId:long}/reviews")]
[Authorize]
public sealed class ReviewsController(OperationsService service) : AuthenticatedController
{
    [HttpGet] public async Task<IActionResult> List(long tripId,CancellationToken ct)=>Ok(await service.Reviews(tripId,CurrentId,CurrentRole==UhilTaxi.Domain.Enums.UserRole.Admin,ct));
    [HttpPost] public async Task<IActionResult> Create(long tripId,[FromBody] ReviewRequest request,CancellationToken ct)=>StatusCode(201,await service.AddReview(tripId,CurrentId,request.Rating,request.Comment,ct));
    public sealed record ReviewRequest(byte Rating,string? Comment);
}
