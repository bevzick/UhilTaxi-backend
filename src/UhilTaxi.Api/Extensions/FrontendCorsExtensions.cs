namespace UhilTaxi.Api.Extensions;

public static class FrontendCorsExtensions
{
    public const string PolicyName = "Frontend";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Cors:AllowedOrigins");
        // A scalar environment variable overrides the JSON array, including an explicitly empty list.
        var origins = section.Value is { } value
            ? value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : section.GetChildren().Select(child => child.Value ?? "").ToArray();

        foreach (var origin in origins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                uri.Host.Contains('*') || uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0 ||
                origin != uri.GetLeftPart(UriPartial.Authority))
                throw new InvalidOperationException("Cors:AllowedOrigins must contain exact HTTP(S) origins without paths or trailing slashes.");
        }

        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
        {
            if (origins.Length > 0) policy.WithOrigins(origins);
            policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }));
        return services;
    }
}
