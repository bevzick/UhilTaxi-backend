using UhilTaxi.Application.Common;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Application.Services;
using UhilTaxi.Application.Validation;
using UhilTaxi.Domain.Constants;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;

namespace UhilTaxi.Tests;

public sealed class OrdersTripsTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Accepted)]
    [InlineData(OrderStatus.Accepted, OrderStatus.DriverArriving)]
    [InlineData(OrderStatus.DriverArriving, OrderStatus.InProgress)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Completed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Accepted, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.DriverArriving, OrderStatus.Cancelled)]
    public void Legal_transition_records_previous_state_and_actor(OrderStatus from, OrderStatus to)
    {
        var order = new Order { Id = 42, Status = from };
        var now = DateTime.UtcNow;
        var history = OrderService.ChangeStatus(order, to, 123, now);
        Assert.Equal(from, history.FromStatus);
        Assert.Equal(to, history.ToStatus);
        Assert.Equal(42, history.OrderId);
        Assert.Equal(123, history.ChangedByUserId);
        Assert.Equal(to, order.Status);
        Assert.Equal(now, order.UpdatedAt);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Completed)]
    [InlineData(OrderStatus.Accepted, OrderStatus.InProgress)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed, OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Accepted)]
    public void Illegal_transition_is_conflict_and_does_not_change_state(OrderStatus from, OrderStatus to)
    {
        var order = new Order { Status = from };
        var error = Assert.Throws<AuthException>(() => OrderService.ChangeStatus(order, to, 1, DateTime.UtcNow));
        Assert.Equal(409, error.Status);
        Assert.Equal("INVALID_ORDER_TRANSITION", error.Code);
        Assert.Equal(from, order.Status);
    }

    [Fact]
    public void Fare_uses_decimal_and_rounds_midpoints_away_from_zero() =>
        Assert.Equal(10.01m, OrderRules.CalculateFare(10, 0.5m, 0, 0.01m, 0));

    [Fact]
    public void Fare_cannot_exceed_mysql_decimal_capacity() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderRules.CalculateFare(OrderRules.MaxFare, 1, 0, 1, 0));

    [Theory]
    [InlineData(-1)]
    [InlineData(1000000)]
    [InlineData(1.001)]
    public void Invalid_distance_is_rejected(decimal distance) =>
        Assert.Equal(400, Assert.Throws<AuthException>(() => OrderValidator.Distance(distance)).Status);

    [Theory]
    [InlineData(91, 26)]
    [InlineData(49, 181)]
    [InlineData(49.12345678, 26)]
    public void Invalid_coordinates_are_rejected(decimal lat, decimal lng)
    {
        var request = Request() with { Pickup = new("Pickup", lat, lng) };
        Assert.Equal(400, Assert.Throws<AuthException>(() => OrderValidator.Validate(request)).Status);
    }

    [Fact]
    public void Missing_coordinate_and_identical_route_are_rejected()
    {
        var request = Request();
        Assert.Throws<AuthException>(() => OrderValidator.Validate(request with { Pickup = request.Pickup with { Lat = null } }));
        Assert.Throws<AuthException>(() => OrderValidator.Validate(request with { Destination = request.Pickup }));
    }

    [Theory]
    [InlineData(UserRole.Client, 2)]
    [InlineData(UserRole.Driver, 3)]
    public void Unrelated_user_cannot_read_order(UserRole role, long actorId)
    {
        var order = new Order { ClientId = 1, AssignedDriverId = 2 };
        Assert.Equal(404, Assert.Throws<AuthException>(() => OrderService.EnsureVisible(order, actorId, role)).Status);
    }

    [Fact]
    public void Percentage_discount_respects_cap_and_minimum()
    {
        var promo = new Promocode { DiscountType = DiscountType.Percentage, DiscountValue = 10, MaxDiscountAmount = 15, MinOrderAmount = 100 };
        Assert.Equal(15m, PromoRules.Discount(promo, 200));
        Assert.Equal(0m, PromoRules.Discount(promo, 99));
    }

    [Fact]
    public void Fixed_discount_never_makes_fare_negative() =>
        Assert.Equal(10m, PromoRules.Discount(new() { DiscountType = DiscountType.Fixed, DiscountValue = 100 }, 10));

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public void Invalid_pagination_is_rejected(int page, int limit) =>
        Assert.Throws<AuthException>(() => OrderValidator.Offset(page, limit));

    private static CreateOrderRequest Request() => new(1, new("Pickup", 49.42m, 26.98m), new("Destination", 49.4m, 27.01m));
}
