using Microsoft.Extensions.Configuration;
namespace UhilTaxi.Api.Extensions;
public static class DotEnvConfiguration
{
    public static ConfigurationManager AddLocalDotEnv(this ConfigurationManager config, string contentRoot)
    {
        // Local dot-env file is never committed. Existing OS environment variables override its values.
        var path = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", ".env"));
        if (!File.Exists(path)) return config;
        var entries = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var separator = line.IndexOf('=');
            if (separator <= 0) continue;
            var name = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"');
            var key = name switch
            {
                "JWT_KEY" => "Jwt:Key",
                "JWT_ISSUER" => "Jwt:Issuer",
                "JWT_AUDIENCE" => "Jwt:Audience",
                "JWT_ACCESS_MINUTES" => "Jwt:AccessMinutes",
                "JWT_REFRESH_DAYS" => "Jwt:RefreshDays",
                _ => name.Replace("__", ":")
            };
            if (Environment.GetEnvironmentVariable(key.Replace(":", "__")) is null)
                entries[key] = value;
        }
        config.AddInMemoryCollection(entries);
        return config;
    }
}
