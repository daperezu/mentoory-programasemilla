using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Infrastructure.Configuration;
using LinaSys.Shared.Infrastructure.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.Shared.Infrastructure;

public static class DependencyInjection
{
    public static void AddSharedInfrastructureServices(this IHostApplicationBuilder builder)
    {
        //// Add Azure Blob Storage client
        builder.AddAzureBlobServiceClient("blobs");

        // Configure storage settings
        builder.Services.Configure<StorageSettings>(options =>
        {
            var configuration = builder.Configuration;

            // Set environment prefix based on environment
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            options.EnvironmentPrefix = environment.ToLowerInvariant() switch
            {
                "production" => "prod",
                "staging" => "staging",
                "development" => "dev",
                _ => "dev"
            };

            // Configure other settings from configuration if available
            var storageSection = configuration.GetSection("Storage");
            if (storageSection.Exists())
            {
                storageSection.Bind(options);
            }
        });

        // Register the generic file storage service
        builder.Services.AddSingleton<IFileStorageService, AzureBlobFileStorageService>();
    }
}
