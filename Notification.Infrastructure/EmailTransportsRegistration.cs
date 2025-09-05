using LinaSys.Notification.Domain.Email;
using LinaSys.Notification.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Infrastructure;

/// <summary>
/// Dependency Injection registration helpers for email transports.
/// Resolves <see cref="IEmailTransport"/> based on environment and configuration:
/// <list type="bullet">
/// <item>
/// <description>Production: <see cref="MailgunApiEmailTransport"/> as primary with optional fallback to <see cref="SmtpEmailTransport"/>.</description>
/// </item>
/// <item>
/// <description>Non-production (e.g., Development/QA): <see cref="SmtpEmailTransport"/> only.</description>
/// </item>
/// </list>
/// </summary>
internal static class EmailTransportsRegistration
{
    /// <summary>
    /// Registers Email transports. In Production: Mailgun API primary + SMTP fallback.
    /// In Development: SMTP only (Mailtrap). You can flip via config/feature flags if desired.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> providing transport and environment settings.</param>
    /// <param name="enableFallbackInProd">When true (default), in Production registers a fallback chain: Mailgun -&gt; SMTP.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <remarks>
    /// Environment selection is based on the <c>ASPNETCORE_ENVIRONMENT</c> configuration value. If it equals
    /// "Production" (case-insensitive), Production strategy is applied; otherwise, non-production strategy is used.
    /// Options are bound from <see cref="MailSmtpOptions.SectionName"/> and <see cref="MailgunOptions.SectionName"/>.
    /// </remarks>
    /// <seealso cref="IEmailTransport"/>
    /// <seealso cref="SmtpEmailTransport"/>
    /// <seealso cref="MailgunApiEmailTransport"/>
    public static IServiceCollection AddEmailTransports(this IServiceCollection services, IConfiguration configuration, bool enableFallbackInProd = true)
    {
        services.Configure<MailSmtpOptions>(configuration.GetSection(MailSmtpOptions.SectionName));
        services.Configure<MailgunOptions>(configuration.GetSection(MailgunOptions.SectionName));

        // Core transports
        services.AddScoped<SmtpEmailTransport>();
        services.AddHttpClient<MailgunApiEmailTransport>();

        // Strategy:
        // - If ASPNETCORE_ENVIRONMENT == Production:
        //      * If enableFallbackInProd: register Fallback(Mailgun -> SMTP) as IEmailTransport
        //      * Else: Mailgun only
        // - Else (Dev/QA): SMTP only (Mailtrap)
        var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var isProd = string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);

        if (isProd)
        {
            if (enableFallbackInProd)
            {
                services.AddScoped<IEmailTransport>(sp =>
                {
                    var primary = sp.GetRequiredService<MailgunApiEmailTransport>();
                    var fallback = sp.GetRequiredService<SmtpEmailTransport>();
                    var logger = sp.GetRequiredService<ILogger<FallbackEmailTransport>>();
                    return new FallbackEmailTransport(primary, fallback, logger);
                });
            }
            else
            {
                services.AddScoped<IEmailTransport>(sp => sp.GetRequiredService<MailgunApiEmailTransport>());
            }
        }
        else
        {
            services.AddScoped<IEmailTransport>(sp => sp.GetRequiredService<SmtpEmailTransport>());
        }

        return services;
    }
}
