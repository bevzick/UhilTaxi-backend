using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UhilTaxi.Application.Abstractions.Auth;
using UhilTaxi.Application.Abstractions.Persistence;
using UhilTaxi.Infrastructure.Auth;
using UhilTaxi.Infrastructure.Persistence;
using UhilTaxi.Infrastructure.Persistence.Repositories;
namespace UhilTaxi.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connection = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Set ConnectionStrings__Default.");
        services.AddDbContext<UhilTaxiDbContext>(o => o.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 36))));
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<ITariffRepository, TariffRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordService, PasswordService>();
        return services;
    }
}
