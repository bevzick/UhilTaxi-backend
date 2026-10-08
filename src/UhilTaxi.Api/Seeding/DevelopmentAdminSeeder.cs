using Microsoft.EntityFrameworkCore;
using UhilTaxi.Application.Abstractions.Auth;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
using UhilTaxi.Infrastructure.Persistence;
namespace UhilTaxi.Api.Seeding;
public static class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        var config = app.Configuration;
        if (!app.Environment.IsDevelopment() || !config.GetValue<bool>("AdminSeed:Enabled")) return;
        var phone = config["AdminSeed:Phone"]?.Trim();
        var password = config["AdminSeed:Password"];
        var firstName = config["AdminSeed:FirstName"]?.Trim();
        var lastName = config["AdminSeed:LastName"]?.Trim();
        var email = config["AdminSeed:Email"]?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(password) || password.Length < 12)
            throw new InvalidOperationException("Admin seed enabled: provide AdminSeed__Phone, Password (12+), FirstName, LastName.");
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UhilTaxiDbContext>();
        // Schema comes from database/init.sql, not EnsureCreated; never recreate production data.
        if (await db.Users.AnyAsync(x => x.Role == UserRole.Admin)) return;
        if (await db.Users.AnyAsync(x => x.Phone == phone || (email != null && x.Email == email)))
            throw new InvalidOperationException("Admin seed identity already belongs to a user. Choose new admin phone/email.");
        var user = new User { Role = UserRole.Admin, Status = UserStatus.Active,
            FirstName = firstName, LastName = lastName, Phone = phone, Email = email };
        user.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordService>().Hash(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        app.Logger.LogInformation("Development admin account created (user id {UserId}). Disable ADMIN_SEED_ENABLED after setup.", user.Id);
    }
}
