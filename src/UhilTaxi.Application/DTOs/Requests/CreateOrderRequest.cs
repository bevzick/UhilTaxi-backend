using System.ComponentModel.DataAnnotations;

namespace UhilTaxi.Application.DTOs.Requests;

public sealed record CreateOrderRequest(
    [Range(1, long.MaxValue)] long TariffId,
    [Required] OrderLocationRequest Pickup,
    [Required] OrderLocationRequest Destination,
    [StringLength(20)] string? Promocode = null);

public sealed record OrderLocationRequest([Required, StringLength(255)] string Address,
    [Required] decimal? Lat, [Required] decimal? Lng);

public sealed record AssignDriverRequest([Range(1, long.MaxValue)] long DriverId);
public sealed record CreateAdminOrderRequest([Range(1, long.MaxValue)] long ClientId,
    [Required] CreateOrderRequest Order);
