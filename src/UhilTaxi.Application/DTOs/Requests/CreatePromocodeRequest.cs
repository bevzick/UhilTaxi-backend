namespace UhilTaxi.Application.DTOs.Requests;
public sealed record CreatePromocodeRequest(string Code, decimal DiscountValue, string DiscountType,
    DateOnly ExpiryDate, int MaxUses, decimal? MinOrderAmount, decimal? MaxDiscountAmount);
