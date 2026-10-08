using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Application.DTOs.Requests;
using UhilTaxi.Application.DTOs.Responses;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
namespace UhilTaxi.Application.Services;
public sealed class TariffService(ITariffRepository repository)
{
    public async Task<List<TariffResponse>> ListAsync(bool activeOnly, CancellationToken ct) =>
        (await repository.ListAsync(activeOnly, ct)).Select(TariffResponse.From).ToList();
    public async Task<TariffResponse?> GetAsync(long id, CancellationToken ct)
    {
        var tariff = await repository.GetAsync(id, ct);
        return tariff is null ? null : TariffResponse.From(tariff);
    }
    public async Task<TariffResponse> CreateAsync(CreateTariffRequest request, CancellationToken ct)
    {
        var name = ValidateName(request.Name);
        var tariff = new Tariff { Name = name, ServiceClass = ParseClass(request.ServiceClass),
            BaseFare = ValidateMoney(request.BaseFare), RatePerKm = ValidateMoney(request.RatePerKm),
            RatePerMin = ValidateMoney(request.RatePerMin) };
        if (await repository.NameExistsAsync(name, null, ct))
            throw new InvalidOperationException("TARIFF_NAME_EXISTS");
        await repository.AddAsync(tariff, ct);
        await repository.SaveAsync(ct);
        return TariffResponse.From(tariff);
    }
    public async Task<TariffResponse?> UpdateAsync(long id, UpdateTariffRequest request, CancellationToken ct)
    {
        var tariff = await repository.GetAsync(id, ct);
        if (tariff is null) return null;
        if (request.Name is not null)
        {
            var name = ValidateName(request.Name);
            if (await repository.NameExistsAsync(name, id, ct)) throw new InvalidOperationException("TARIFF_NAME_EXISTS");
            tariff.Name = name;
        }
        if (request.ServiceClass is not null) tariff.ServiceClass = ParseClass(request.ServiceClass);
        if (request.BaseFare.HasValue) tariff.BaseFare = ValidateMoney(request.BaseFare.Value);
        if (request.RatePerKm.HasValue) tariff.RatePerKm = ValidateMoney(request.RatePerKm.Value);
        if (request.RatePerMin.HasValue) tariff.RatePerMin = ValidateMoney(request.RatePerMin.Value);
        tariff.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return TariffResponse.From(tariff);
    }
    public async Task<TariffResponse?> SetStatusAsync(long id, bool isActive, CancellationToken ct)
    {
        var tariff = await repository.GetAsync(id, ct);
        if (tariff is null) return null;
        tariff.IsActive = isActive;
        tariff.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return TariffResponse.From(tariff);
    }
    private static string ValidateName(string? name) => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 50
        ? name.Trim() : throw new ArgumentException("Tariff name must have 1-50 characters.");
    private static decimal ValidateMoney(decimal value) => value >= 0 && value <= 99999999.99m && decimal.Round(value, 2) == value
        ? value : throw new ArgumentException("Amount must be non-negative and have no more than 2 decimal places.");
    private static TariffServiceClass ParseClass(string? value) => value?.ToLowerInvariant() switch
    {
        "economy" => TariffServiceClass.Economy,
        "standard" => TariffServiceClass.Standard,
        "business" => TariffServiceClass.Business,
        "xl" => TariffServiceClass.Xl,
        _ => throw new ArgumentException("Service class must be economy, standard, business, or xl.")
    };
}
