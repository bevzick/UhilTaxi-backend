using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaxiPark.Application.Abstractions.Auth;
using TaxiPark.Application.Abstractions.Persistence;
using TaxiPark.Infrastructure.Auth;
using TaxiPark.Infrastructure.Persistence;
using TaxiPark.Infrastructure.Persistence.Repositories;
namespace TaxiPark.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connection = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Set ConnectionStrings__Default.");
        services.AddDbContext<TaxiParkDbContext>(o => o.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 36))));
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordService, PasswordService>();
        return services;
    }
}
