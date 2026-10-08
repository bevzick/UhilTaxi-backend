using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Application.Services;
public sealed class PromocodeService(IPromocodeRepository repository)
{
    private static AuthException Invalid(string code, string message) => new(400, code, message);
    private static string ParseCode(string? code)
    {
        var value = code?.Trim().ToUpperInvariant() ?? "";
        if (value.Length < 1 || value.Length > 20 || !value.All(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '-'))
            throw Invalid("INVALID_PROMOCODE", "Code must contain 1-20 Latin letters, numbers, '-' or '_'.");
        return value;
    }
    private static DiscountType ParseType(string? type) => type?.Trim().ToLowerInvariant() switch
    {
        "fixed" => DiscountType.Fixed,
        "percentage" => DiscountType.Percentage,
        _ => throw Invalid("INVALID_DISCOUNT_TYPE", "Discount type must be fixed or percentage.")
    };
    private static decimal Amount(decimal value, string name, bool strictlyPositive = false)
    {
        if ((strictlyPositive ? value <= 0 : value < 0) || value > 99999999.99m || decimal.Round(value,2) != value)
            throw Invalid("INVALID_AMOUNT", $"{name} must be a valid amount with up to two decimals.");
        return value;
    }
    private static void Validate(Promocode p)
    {
        if (p.ExpiryDate == default) throw Invalid("INVALID_EXPIRY_DATE", "Expiry date is required.");
        if (p.MaxUses < 1) throw Invalid("INVALID_MAX_USES", "MaxUses must be at least 1.");
        if (p.DiscountType == DiscountType.Fixed) Amount(p.DiscountValue, "DiscountValue", true);
        else if (p.DiscountValue <= 0 || p.DiscountValue > 100 || decimal.Round(p.DiscountValue, 2) != p.DiscountValue)
            throw Invalid("INVALID_PERCENT", "Percentage must be greater than 0 and at most 100.");
        if (p.MinOrderAmount.HasValue) Amount(p.MinOrderAmount.Value, "MinOrderAmount");
        if (p.MaxDiscountAmount.HasValue) Amount(p.MaxDiscountAmount.Value, "MaxDiscountAmount");
    }
    private async Task<PromocodeResponse> MapAsync(Promocode p, CancellationToken ct) =>
        PromocodeResponse.From(p, await repository.UsesCountAsync(p.Id, ct));
    public async Task<List<PromocodeResponse>> ListAsync(CancellationToken ct)
    {
        var result = new List<PromocodeResponse>();
        foreach (var p in await repository.ListAsync(ct)) result.Add(await MapAsync(p, ct));
        return result;
    }
    public async Task<PromocodeResponse?> GetAsync(long id, CancellationToken ct) =>
        await repository.FindAsync(id, ct) is { } p ? await MapAsync(p, ct) : null;
    public async Task<PromocodeResponse> CreateAsync(CreatePromocodeRequest r, CancellationToken ct)
    {
        var code = ParseCode(r.Code);
        if (await repository.CodeExistsAsync(code, null, ct)) throw new AuthException(409, "PROMOCODE_EXISTS", "Promocode already exists.");
        var p = new Promocode { Code = code, DiscountValue = r.DiscountValue,
            DiscountType = ParseType(r.DiscountType), ExpiryDate = r.ExpiryDate, MaxUses = r.MaxUses,
            MinOrderAmount = r.MinOrderAmount, MaxDiscountAmount = r.MaxDiscountAmount };
        Validate(p);
        repository.Add(p);
        await repository.SaveAsync(ct);
        return await MapAsync(p, ct);
    }
    public async Task<PromocodeResponse?> UpdateAsync(long id, UpdatePromocodeRequest r, CancellationToken ct)
    {
        var p = await repository.FindAsync(id, ct);
        if (p is null) return null;
        if (r.Code is not null)
        {
            var code = ParseCode(r.Code);
            if (await repository.CodeExistsAsync(code, id, ct)) throw new AuthException(409, "PROMOCODE_EXISTS", "Promocode already exists.");
            p.Code = code;
        }
        if (r.DiscountValue.HasValue) p.DiscountValue = r.DiscountValue.Value;
        if (r.DiscountType is not null) p.DiscountType = ParseType(r.DiscountType);
        if (r.ExpiryDate.HasValue) p.ExpiryDate = r.ExpiryDate.Value;
        if (r.MaxUses.HasValue) p.MaxUses = r.MaxUses.Value;
        if (r.MinOrderAmount.HasValue) p.MinOrderAmount = r.MinOrderAmount;
        if (r.MaxDiscountAmount.HasValue) p.MaxDiscountAmount = r.MaxDiscountAmount;
        Validate(p);
        var used = await repository.UsesCountAsync(id, ct);
        if (p.MaxUses < used) throw new AuthException(409, "MAX_USES_TOO_LOW", "MaxUses cannot be lower than existing usage count.");
        p.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return PromocodeResponse.From(p, used);
    }
    public async Task<PromocodeResponse?> SetStatusAsync(long id, bool active, CancellationToken ct)
    {
        var p = await repository.FindAsync(id, ct);
        if (p is null) return null;
        p.IsActive = active;
        p.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return await MapAsync(p, ct);
    }
}
