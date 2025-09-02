using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LinaSys.Aspire.AppHost;

public static class AppsettingsLoader
{
    public static IConfiguration LoadJsonConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Ensure correct path resolution
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false) // Base config
#if DEBUG
                .AddJsonFile($"appsettings.{environment}.json", optional: true,
                    reloadOnChange: false) // Environment override
#endif
            ;

        return builder.Build();
    }

    public static string SerializeUserJsonConfiguration()
    {
        var appSettingsConfig = LoadJsonConfiguration().AsEnumerable()
            .Where(kv => kv.Value is not null
                         && !kv.Key.StartsWith("ConnectionStrings")
                         && !kv.Key.StartsWith("Logging"))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var json = JsonSerializer.Serialize(appSettingsConfig);
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
    }
}
