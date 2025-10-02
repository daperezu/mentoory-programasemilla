using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddAspireConfiguration();

        // Only configure OpenTelemetry if explicitly enabled or in development
        var enableTelemetry = builder.Configuration.GetValue<bool>("ENABLE_TELEMETRY", false);
        if (enableTelemetry || builder.Environment.IsDevelopment())
        {
            builder.ConfigureOpenTelemetry();
        }

        builder.AddDefaultHealthChecks();

        // Service Discovery is only needed for microservices, not monoliths
        var enableServiceDiscovery = builder.Configuration.GetValue<bool>("ENABLE_SERVICE_DISCOVERY", false);
        if (enableServiceDiscovery || builder.Environment.IsDevelopment())
        {
            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });
        }

        return builder;
    }

    public static void AddAspireConfiguration<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var configValue = builder.Configuration["AspireAppsettings"];

        if (configValue is null)
        {
            return;
        }

        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(configValue));

        var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        builder.Configuration.AddJsonStream(mem);
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // Check if we should use lightweight logging only (no OpenTelemetry overhead)
        var lightweightLogging = builder.Configuration.GetValue<bool>("LIGHTWEIGHT_LOGGING", true);
        if (lightweightLogging && !builder.Environment.IsDevelopment())
        {
            // Just use standard ILogger without OpenTelemetry overhead
            return builder;
        }

        // Always add OpenTelemetry logging if not in lightweight mode
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        // Check if metrics/tracing should be enabled
        var enableMetrics = builder.Configuration.GetValue<bool>("APPLICATIONINSIGHTS_ENABLE_METRICS", false);
        var enableTracing = builder.Configuration.GetValue<bool>("APPLICATIONINSIGHTS_ENABLE_TRACING", false);

        // Only add metrics and tracing if explicitly enabled or if running locally with OTLP
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (enableMetrics || enableTracing || useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    if (enableMetrics || useOtlpExporter)
                    {
                        metrics.AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddRuntimeInstrumentation();
                    }
                })
                .WithTracing(tracing =>
                {
                    if (enableTracing || useOtlpExporter)
                    {
                        tracing.AddSource(builder.Environment.ApplicationName)
                            .AddAspNetCoreInstrumentation()
                            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                            // .AddGrpcClientInstrumentation()
                            .AddHttpClientInstrumentation();
                    }
                });

            if (enableTracing || useOtlpExporter)
            {
                builder.Services.AddScoped(sp =>
                {
                    var provider = sp.GetRequiredService<TracerProvider>();
                    return provider.GetTracer(builder.Environment.ApplicationName);
                });
            }
        }

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()

            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live"),
            });
        }

        return app;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        var useAzureMonitor = !string.IsNullOrEmpty(connectionString);

        // Check what telemetry types are enabled
        var enableMetrics = builder.Configuration.GetValue<bool>("APPLICATIONINSIGHTS_ENABLE_METRICS", false);
        var enableTracing = builder.Configuration.GetValue<bool>("APPLICATIONINSIGHTS_ENABLE_TRACING", false);

        // Use Azure Monitor in production, OTLP (Aspire Dashboard) in development
        if (useAzureMonitor)
        {
            // Only add Azure Monitor if we have metrics or tracing enabled
            // For logging-only scenarios, use the built-in Application Insights SDK directly
            if (enableMetrics || enableTracing)
            {
                builder.Services.AddOpenTelemetry()
                    .UseAzureMonitor(options =>
                    {
                        options.ConnectionString = connectionString;

                        // Disable expensive features for cost optimization
                        options.EnableLiveMetrics = false; // Disable real-time metrics streaming
                    });

                // Configure sampling for cost reduction (applies to all telemetry types)
                if (!builder.Environment.IsDevelopment())
                {
                    builder.Services.Configure<Azure.Monitor.OpenTelemetry.AspNetCore.AzureMonitorOptions>(options =>
                    {
                        // Get sampling percentage from environment variable or default to 5%
                        var samplingPercentageStr = builder.Configuration["APPLICATIONINSIGHTS_SAMPLING_PERCENTAGE"];
                        if (float.TryParse(samplingPercentageStr, out var samplingPercentage))
                        {
                            options.SamplingRatio = samplingPercentage / 100f;
                        }
                        else
                        {
                            options.SamplingRatio = 0.05f; // Default to 5% sampling
                        }
                    });
                }
            }

            // For production logging-only scenarios without metrics/tracing,
            // the application will use standard ILogger which automatically
            // sends to Application Insights if the connection string is present
        }
        else
        {
            // Use OTLP exporter for local Aspire dashboard (all telemetry types enabled for development)
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }
        }

        return builder;
    }
}