using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.Common;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Application.Validation;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
using System.Text.Json;

namespace UhilTaxi.Application.Services;

public sealed class TripService(IOrderRepository orders, ITripRepository trips, TimeProvider clock)
{
    public Task<TripResponse> StartAsync(long orderId, long driverId, StartTripRequest request, CancellationToken ct, long? adminId = null) =>
        orders.InTransactionAsync(async () =>
        {
            if (!await orders.LockDriverAsync(driverId, ct))
                throw new AuthException(409, "DRIVER_NOT_ACTIVE", "An active driver profile is required.");
            var order = await orders.GetForUpdateAsync(orderId, ct);
            OrderService.EnsureVisible(order, driverId, UserRole.Driver);
            if (order!.Status != OrderStatus.DriverArriving)
                throw new AuthException(409, "INVALID_ORDER_TRANSITION", "Order must be in driver_arriving state before starting.");
            var shift = await trips.GetShiftForUpdateAsync(request.ShiftId, ct);
            if (shift is null || shift.DriverId != driverId || shift.Status != "open" || shift.EndTime is not null ||
                shift.StartTime > OrderService.UtcNow(clock))
                throw new AuthException(409, "OPEN_SHIFT_REQUIRED", "Choose your current open shift.");
            var tariff = await orders.GetTariffAsync(order.TariffId, ct)
                ?? throw new AuthException(409, "TARIFF_NOT_FOUND", "Order tariff is unavailable.");
            if (await trips.CarCategoryAsync(shift.CarId, ct) != tariff.ServiceClass.ToString().ToLowerInvariant())
                throw new AuthException(409, "CAR_CLASS_MISMATCH", "An active car matching the tariff class is required.");
            var now = OrderService.UtcNow(clock);
            var trip = new Trip
            {
                OrderId = order.Id, ShiftId = shift.Id, ActualStartTime = now,
                TariffName = tariff.Name, BaseFare = tariff.BaseFare, RatePerKm = tariff.RatePerKm,
                RatePerMin = tariff.RatePerMin, CreatedAt = now, UpdatedAt = now
            };
            trips.Add(trip);
            orders.AddHistory(OrderService.ChangeStatus(order, OrderStatus.InProgress, adminId ?? driverId, now));
            order.StartedAt = now;
            if (adminId is { } admin)
                await orders.AuditAsync(admin, "order.start", orderId, JsonSerializer.Serialize(new { shift_id = shift.Id }), now, ct);
            await orders.SaveAsync(ct);
            return TripResponse.From(trip);
        }, ct);

    public Task<TripResponse> CompleteAsync(long orderId, long driverId, CompleteTripRequest request, CancellationToken ct, long? adminId = null) =>
        orders.InTransactionAsync(async () =>
        {
            if (request.DistanceKm is null)
                throw new AuthException(400, "INVALID_DISTANCE", "Actual distance is required.");
            OrderValidator.Distance(request.DistanceKm.Value);
            var order = await orders.GetForUpdateAsync(orderId, ct);
            OrderService.EnsureVisible(order, driverId, UserRole.Driver);
            if (order!.Status != OrderStatus.InProgress)
                throw new AuthException(409, "INVALID_ORDER_TRANSITION", "Only an in-progress trip can be completed.");
            var trip = await trips.GetByOrderAsync(orderId, ct)
                ?? throw new AuthException(409, "TRIP_NOT_STARTED", "Trip has not been started.");
            if (trip.ActualEndTime is not null)
                throw new AuthException(409, "TRIP_ALREADY_COMPLETED", "Trip has already been completed.");
            var now = OrderService.UtcNow(clock);
            var elapsed = Math.Ceiling((now - trip.ActualStartTime).TotalMinutes);
            if (elapsed < 0 || elapsed > int.MaxValue)
                throw new AuthException(409, "INVALID_TRIP_TIME", "Trip duration is outside supported limits.");
            var duration = (int)elapsed;
            var amount = OrderService.Fare(trip.BaseFare, trip.RatePerKm, trip.RatePerMin, request.DistanceKm.Value, duration);
            if (order.PromocodeId is { } promoId)
            {
                var promo = await orders.GetPromocodeAsync(promoId, ct)
                    ?? throw new AuthException(409, "PROMOCODE_NOT_FOUND", "Reserved promocode is unavailable.");
                // The usage was reserved at order creation; expiry does not revoke that reservation.
                trip.DiscountAmount = PromoRules.Discount(promo, amount);
            }
            trip.DistanceKm = request.DistanceKm;
            trip.DurationMin = duration;
            trip.FinalFare = amount - trip.DiscountAmount;
            trip.ActualEndTime = now;
            trip.UpdatedAt = now;
            orders.AddHistory(OrderService.ChangeStatus(order, OrderStatus.Completed, adminId ?? driverId, now));
            order.CompletedAt = now;
            if (adminId is { } admin)
                await orders.AuditAsync(admin, "order.complete", orderId, JsonSerializer.Serialize(new { final_fare = trip.FinalFare }), now, ct);
            await orders.SaveAsync(ct);
            return TripResponse.From(trip);
        }, ct);

    public async Task<TripResponse> GetAsync(long id, long actorId, UserRole role, CancellationToken ct)
    {
        var trip = await trips.GetAsync(id, ct)
            ?? throw new AuthException(404, "TRIP_NOT_FOUND", "Trip was not found.");
        OrderService.EnsureVisible(trip.Order, actorId, role);
        return TripResponse.From(trip);
    }

    public async Task<PagedResponse<TripResponse>> ListAsync(long actorId, UserRole role,
        int page, int limit, string sort, CancellationToken ct)
    {
        var skip = OrderValidator.Offset(page, limit);
        OrderService.ValidateSort(sort);
        var (data, total) = await trips.ListAsync(role == UserRole.Client ? actorId : null,
            role == UserRole.Driver ? actorId : null, skip, limit, sort, ct);
        return PagedResponse<TripResponse>.Create(data.Select(TripResponse.From).ToList(), total, page, limit);
    }
}
