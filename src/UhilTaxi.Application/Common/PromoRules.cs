using UhilTaxi.Application.Exceptions;
using UhilTaxi.Domain.Entities;

namespace UhilTaxi.Application.Common;

public static class PromoRules
{
    public static decimal Discount(Promocode promo, decimal amount)
    {
        if (promo.DiscountValue <= 0 || promo.DiscountType is not ("fixed" or "percentage") ||
            (promo.DiscountType == "percentage" && promo.DiscountValue > 100) ||
            promo.MinOrderAmount < 0 || promo.MaxDiscountAmount < 0)
            throw new AuthException(409, "INVALID_PROMOCODE", "Promocode configuration is invalid.");
        if (promo.MinOrderAmount is { } minimum && amount < minimum) return 0;
        var discount = promo.DiscountType == "percentage" ? amount * promo.DiscountValue / 100 : promo.DiscountValue;
        if (promo.MaxDiscountAmount is { } maximum) discount = Math.Min(discount, maximum);
        return decimal.Round(Math.Min(amount, discount), 2, MidpointRounding.AwayFromZero);
    }
}
