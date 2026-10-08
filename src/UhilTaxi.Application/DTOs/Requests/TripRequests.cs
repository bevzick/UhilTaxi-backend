using System.ComponentModel.DataAnnotations;

namespace UhilTaxi.Application.DTOs.Requests;

public sealed record StartTripRequest([Range(1, long.MaxValue)] long ShiftId);
public sealed record CompleteTripRequest([Required] decimal? DistanceKm);
public sealed record CancelOrderRequest([Required, StringLength(500)] string Reason);
