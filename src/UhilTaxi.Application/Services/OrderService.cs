using UhilTaxi.Application.Abstractions.External;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.Common;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Application.Validation;
using UhilTaxi.Domain.Constants;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
using System.Text.Json;

namespace UhilTaxi.Application.Services;

public sealed class OrderService(IOrderRepository orders, IMapsService maps, TimeProvider clock)
{
    public async Task<OrderEstimateResponse> EstimateAsync(CreateOrderRequest request, CancellationToken ct) =>
        (await QuoteAsync(request, false, ct)).Estimate;

    public Task<OrderResponse> CreateAsync(long clientId, CreateOrderRequest request, CancellationToken ct, long? adminId = null) =>
        orders.InTransactionAsync(async () =>
        {
            if (!await orders.ClientExistsAsync(clientId, ct))
                throw new AuthException(403, "CLIENT_REQUIRED", "An active client profile is required.");
            var quote = await QuoteAsync(request, true, ct);
            var now = UtcNow(clock);
            var order = new Order
            {
                ClientId = clientId, TariffId = request.TariffId, PromocodeId = quote.Promo?.Id,
                PickupAddress = request.Pickup.Address.Trim(), PickupLat = request.Pickup.Lat!.Value,
                PickupLng = request.Pickup.Lng!.Value, DestinationAddress = request.Destination.Address.Trim(),
                DestinationLat = request.Destination.Lat!.Value, DestinationLng = request.Destination.Lng!.Value,
                EstimatedDistanceKm = quote.Estimate.DistanceKm, EstimatedDurationMin = quote.Estimate.DurationMin,
                EstimatedFare = quote.Estimate.EstimatedFare, CreatedAt = now, UpdatedAt = now
            };
            orders.Add(order);
            orders.AddHistory(new OrderStatusHistory
            {
                Order = order, ToStatus = OrderStatus.Pending, ChangedByUserId = adminId ?? clientId, CreatedAt = now
            });
            if (quote.Promo is { } promo)
                orders.AddPromoUsage(new PromocodeUsage
                {
                    PromocodeId = promo.Id, ClientId = clientId, Order = order, UsedAt = now
                });
            await orders.SaveAsync(ct);
            if (adminId is { } admin)
                await orders.AuditAsync(admin, "order.create", order.Id, JsonSerializer.Serialize(new { client_id = clientId }), now, ct);
            return OrderResponse.From(order);
        }, ct);

    public async Task<PagedResponse<OrderResponse>> ListAsync(long actorId, UserRole role,
        int page, int limit, string? status, string sort, bool available, CancellationToken ct)
    {
        var skip = OrderValidator.Offset(page, limit);
        ValidateSort(sort);
        OrderStatus? parsed = null;
        if (status is not null)
        {
            try { parsed = OrderRules.ParseStatus(status); }
            catch (ArgumentException) { throw new AuthException(400, "INVALID_STATUS", "Unknown order status."); }
        }
        var (data, total) = await orders.ListAsync(new(
            role == UserRole.Client ? actorId : null, role == UserRole.Driver ? actorId : null,
            parsed, available, skip, limit, sort), ct);
        return PagedResponse<OrderResponse>.Create(data.Select(OrderResponse.From).ToList(), total, page, limit);
    }

    public async Task<OrderResponse> GetAsync(long id, long actorId, UserRole role, CancellationToken ct)
    {
        var order = await orders.GetAsync(id, ct);
        EnsureVisible(order, actorId, role);
        return OrderResponse.From(order!);
    }

    public async Task<List<OrderHistoryResponse>> HistoryAsync(long id, long actorId, UserRole role, CancellationToken ct)
    {
        EnsureVisible(await orders.GetAsync(id, ct), actorId, role);
        return (await orders.HistoryAsync(id, ct)).Select(OrderHistoryResponse.From).ToList();
    }

    public Task<OrderResponse> AcceptAsync(long id, long driverId, CancellationToken ct, long? adminId = null) =>
        orders.InTransactionAsync(async () =>
        {
            await RequireDriverAsync(driverId, ct);
            var order = await orders.GetForUpdateAsync(id, ct) ?? throw Missing();
            if (order.Status != OrderStatus.Pending ||
                (order.AssignedDriverId is { } assigned && assigned != driverId))
                throw new AuthException(409, "ORDER_NOT_PENDING", "Order is no longer available for this driver.");
            await RequireAvailableDriverAsync(driverId, order.TariffId, ct);
            order.AssignedDriverId = driverId;
            var now = UtcNow(clock);
            orders.AddHistory(ChangeStatus(order, OrderStatus.Accepted, adminId ?? driverId, now));
            order.AcceptedAt = now;
            if (adminId is { } admin)
                await orders.AuditAsync(admin, "order.accept", id, JsonSerializer.Serialize(new { driver_id = driverId }), now, ct);
            await orders.SaveAsync(ct);
            return OrderResponse.From(order);
        }, ct);

    public Task<OrderResponse> ArrivedAsync(long id, long driverId, CancellationToken ct, long? adminId = null) =>
        orders.InTransactionAsync(async () =>
        {
            var order = await orders.GetForUpdateAsync(id, ct);
            EnsureVisible(order, driverId, UserRole.Driver);
            var now = UtcNow(clock);
            orders.AddHistory(ChangeStatus(order!, OrderStatus.DriverArriving, adminId ?? driverId, now));
            order!.ArrivedAt = now;
            if (adminId is { } admin)
                await orders.AuditAsync(admin, "order.arrived", id, "{}", now, ct);
            await orders.SaveAsync(ct);
            return OrderResponse.From(order);
        }, ct);

    public Task<OrderResponse> CancelAsync(long id, long actorId, UserRole role, string? reason, CancellationToken ct) =>
        orders.InTransactionAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
                throw new AuthException(400, "INVALID_CANCELLATION_REASON", "Provide a reason with 1-500 characters.");
            var order = await orders.GetForUpdateAsync(id, ct);
            EnsureVisible(order, actorId, role);
            var now = UtcNow(clock);
            orders.AddHistory(ChangeStatus(order!, OrderStatus.Cancelled, actorId, now, reason.Trim()));
            order!.CancellationReason = reason.Trim();
            order.CancelledByUserId = actorId;
            order.CancelledAt = now;
            if (role == UserRole.Admin)
                await orders.AuditAsync(actorId, "order.cancel", id, JsonSerializer.Serialize(new { reason = reason.Trim() }), now, ct);
            await orders.SaveAsync(ct);
            return OrderResponse.From(order);
        }, ct);

    public Task<OrderResponse> AssignAsync(long id, long adminId, long driverId, CancellationToken ct) =>
        orders.InTransactionAsync(async () =>
        {
            await RequireDriverAsync(driverId, ct);
            var order = await orders.GetForUpdateAsync(id, ct) ?? throw Missing();
            if (order.Status != OrderStatus.Pending)
                throw new AuthException(409, "ORDER_NOT_PENDING", "Only a pending order can be assigned.");
            await RequireAvailableDriverAsync(driverId, order.TariffId, ct);
            order.AssignedDriverId = driverId;
            order.UpdatedAt = UtcNow(clock);
            // Assignment keeps the status pending, but records the administrative action.
            orders.AddHistory(new OrderStatusHistory
            {
                OrderId = id, FromStatus = order.Status, ToStatus = order.Status,
                ChangedByUserId = adminId, Reason = $"Assigned driver {driverId}.", CreatedAt = order.UpdatedAt
            });
            await orders.AuditAsync(adminId, "order.assign_driver", id,
                JsonSerializer.Serialize(new { driver_id = driverId }), order.UpdatedAt, ct);
            await orders.SaveAsync(ct);
            return OrderResponse.From(order);
        }, ct);

    private async Task<(OrderEstimateResponse Estimate, Promocode? Promo)> QuoteAsync(
        CreateOrderRequest request, bool reservePromo, CancellationToken ct)
    {
        OrderValidator.Validate(request);
        var tariff = await orders.GetTariffAsync(request.TariffId, ct);
        if (tariff is null || !tariff.IsActive)
            throw new AuthException(400, "TARIFF_NOT_ACTIVE", "Choose an active tariff.");
        var route = await maps.EstimateAsync(request.Pickup.Lat!.Value, request.Pickup.Lng!.Value,
            request.Destination.Lat!.Value, request.Destination.Lng!.Value, ct);
        var amount = Fare(tariff.BaseFare, tariff.RatePerKm, tariff.RatePerMin, route.DistanceKm, route.DurationMin);
        Promocode? promo = null;
        var discount = 0m;
        if (request.Promocode is { } code)
        {
            promo = await orders.GetPromocodeAsync(code.Trim().ToUpperInvariant(), reservePromo, ct);
            if (promo is null || !promo.IsActive || promo.ExpiryDate < DateOnly.FromDateTime(UtcNow(clock)))
                throw new AuthException(400, "PROMOCODE_NOT_VALID", "Promocode is inactive, expired, or unknown.");
            if (promo.MaxUses <= await orders.PromoUsageCountAsync(promo.Id, ct))
                throw new AuthException(409, "PROMOCODE_EXHAUSTED", "Promocode usage limit has been reached.");
            if (promo.MinOrderAmount is { } minimum && amount < minimum)
                throw new AuthException(400, "PROMOCODE_MIN_AMOUNT", "Order does not meet the promocode minimum amount.");
            discount = PromoRules.Discount(promo, amount);
        }
        return (new(route.DistanceKm, route.DurationMin, amount, discount, amount - discount), promo);
    }

    private async Task RequireDriverAsync(long driverId, CancellationToken ct)
    {
        if (!await orders.LockDriverAsync(driverId, ct))
            throw new AuthException(409, "DRIVER_NOT_ACTIVE", "An active driver profile is required.");
    }

    private async Task RequireAvailableDriverAsync(long driverId, long tariffId, CancellationToken ct)
    {
        if (await orders.HasActiveOrderAsync(driverId, ct))
            throw new AuthException(409, "DRIVER_BUSY", "Driver already has an active order.");
        var category = await orders.OpenShiftCategoryAsync(driverId, ct);
        if (category is null)
            throw new AuthException(409, "OPEN_SHIFT_REQUIRED", "Driver needs one open shift with an active car.");
        var tariff = await orders.GetTariffAsync(tariffId, ct);
        if (tariff is null || category != tariff.ServiceClass.ToString().ToLowerInvariant())
            throw new AuthException(409, "CAR_CLASS_MISMATCH", "Car class must match the order tariff.");
    }

    public static OrderStatusHistory ChangeStatus(Order order, OrderStatus next, long actorId, DateTime now, string? reason = null)
    {
        if (!OrderRules.CanTransition(order.Status, next))
            throw new AuthException(409, "INVALID_ORDER_TRANSITION", "This action is not allowed in the current order state.");
        var history = new OrderStatusHistory
        {
            OrderId = order.Id, FromStatus = order.Status, ToStatus = next,
            ChangedByUserId = actorId, Reason = reason, CreatedAt = now
        };
        order.Status = next;
        order.UpdatedAt = now;
        return history;
    }

    public static void EnsureVisible(Order? order, long actorId, UserRole role)
    {
        if (order is null || (role == UserRole.Client && order.ClientId != actorId) ||
            (role == UserRole.Driver && order.AssignedDriverId != actorId)) throw Missing();
    }

    public static void ValidateSort(string sort)
    {
        if (sort is not ("created_at" or "-created_at" or "id" or "-id"))
            throw new AuthException(400, "INVALID_SORT", "Sort must be created_at, -created_at, id, or -id.");
    }

    public static decimal Fare(decimal baseFare, decimal perKm, decimal perMin, decimal distance, int duration)
    {
        try { return OrderRules.CalculateFare(baseFare, perKm, perMin, distance, duration); }
        catch (ArgumentOutOfRangeException) { throw new AuthException(400, "INVALID_FARE", "Fare inputs exceed supported limits."); }
    }

    public static DateTime UtcNow(TimeProvider clock)
    {
        var utc = clock.GetUtcNow().UtcDateTime;
        return new DateTime(utc.Ticks - utc.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
    }

    private static AuthException Missing() => new(404, "ORDER_NOT_FOUND", "Order was not found.");
}
