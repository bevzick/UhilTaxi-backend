using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.DTOs.Responses;
public sealed record PromocodeResponse(long Id, string Code, decimal DiscountValue, string DiscountType,
    DateOnly ExpiryDate, int MaxUses, int UsesCount, bool IsActive, decimal? MinOrderAmount,
    decimal? MaxDiscountAmount, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static PromocodeResponse From(Promocode p, int uses) => new(p.Id, p.Code, p.DiscountValue,
        p.DiscountType.ToString().ToLowerInvariant(), p.ExpiryDate, p.MaxUses, uses, p.IsActive,
        p.MinOrderAmount, p.MaxDiscountAmount, p.CreatedAt, p.UpdatedAt);
}
