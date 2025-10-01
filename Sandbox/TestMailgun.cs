using LinaSys.Notification.Domain.Email;
using LinaSys.Notification.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinaSys.Sandbox;

/// <summary>
/// Simple test class to verify Mailgun email sending functionality.
/// Run this with: dotnet run --project Sandbox -- mailgun.
/// </summary>
public static class TestMailgun
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Mailgun Email Test ===\n");

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("../Aspire.AppHost/appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Set up services
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        services.Configure<MailgunOptions>(configuration.GetSection(MailgunOptions.SectionName));
        services.AddHttpClient<MailgunApiEmailTransport>();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Get configuration
            var mailgunOptions = serviceProvider.GetRequiredService<IOptions<MailgunOptions>>().Value;

            Console.WriteLine("Configuration:");
            Console.WriteLine($"  Domain: {mailgunOptions.Domain}");
            Console.WriteLine($"  From: {mailgunOptions.FromName} <{mailgunOptions.FromAddress}>");
            Console.WriteLine($"  Test Mode: {mailgunOptions.TestMode}");
            Console.WriteLine($"  API Key: {(string.IsNullOrEmpty(mailgunOptions.ApiKey) ? "NOT SET" : "SET (hidden)")}\n");

            if (string.IsNullOrEmpty(mailgunOptions.ApiKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: API Key is not configured!");
                Console.ResetColor();
                Console.WriteLine("\nPlease update the Mailgun configuration in appsettings.Development.json:");
                Console.WriteLine("1. Log into your Mailgun account");
                Console.WriteLine("2. Go to Sending → Domains");
                Console.WriteLine("3. Copy your domain name (usually starts with 'mg.' or 'sandbox')");
                Console.WriteLine("4. Go to API Keys section and copy your Private API key");
                Console.WriteLine("5. Update the configuration file with these values");
                return;
            }

            // Create email transport
            var logger = serviceProvider.GetRequiredService<ILogger<MailgunApiEmailTransport>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(MailgunApiEmailTransport));

            var transport = new MailgunApiEmailTransport(
                httpClient,
                Options.Create(mailgunOptions),
                logger);

            // Create test email
            var email = new EmailEnvelope(
                To: "test@example.com", // Will be overridden in transport
                Subject: $"LinaSys Mailgun Test - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                HtmlBody: $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                            .content {{ padding: 20px; background-color: #f8f9fa; }}
                            .info {{ background-color: white; padding: 15px; margin: 10px 0; border-left: 4px solid #007bff; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Mailgun Test Email</h1>
                            </div>
                            <div class='content'>
                                <h2>Test Successful! 🎉</h2>
                                <p>If you're reading this, the Mailgun integration is working correctly.</p>
                                
                                <div class='info'>
                                    <h3>Test Details:</h3>
                                    <ul>
                                        <li><strong>Timestamp:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                                        <li><strong>Domain:</strong> {mailgunOptions.Domain}</li>
                                        <li><strong>From:</strong> {mailgunOptions.FromAddress}</li>
                                        <li><strong>Test Mode:</strong> {mailgunOptions.TestMode}</li>
                                        <li><strong>Environment:</strong> Sandbox Test</li>
                                    </ul>
                                </div>
                                
                                <p>This email was sent using the Mailgun API integration in LinaSys.</p>
                            </div>
                        </div>
                    </body>
                    </html>",
                TextBody: "Test email from LinaSys Mailgun integration.",
                Tag: "test");

            Console.WriteLine("Sending test email...\n");

            // Send email
            await transport.SendAsync(email);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Email sent successfully!");
            Console.ResetColor();

            Console.WriteLine($"\nEmail Details:");
            Console.WriteLine($"  To: danny.perez.u@gmail.com (hardcoded in transport)");
            Console.WriteLine($"  Subject: {email.Subject}");
            Console.WriteLine($"  Tag: {email.Tag}");

            if (mailgunOptions.TestMode)
            {
                Console.WriteLine("\n⚠️  Note: Test mode is enabled - email was validated but not actually sent");
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ HTTP Error: {httpEx.Message}");
            Console.ResetColor();

            if (httpEx.Message.Contains("401"))
            {
                Console.WriteLine("\n401 Unauthorized - This usually means:");
                Console.WriteLine("  1. The API key is invalid or expired");
                Console.WriteLine("  2. The domain doesn't exist in your Mailgun account");
                Console.WriteLine("  3. The domain doesn't match the API key");
                Console.WriteLine("\nPlease verify your Mailgun credentials.");
            }
            else if (httpEx.Message.Contains("404"))
            {
                Console.WriteLine("\n404 Not Found - This usually means:");
                Console.WriteLine("  1. The domain is incorrect");
                Console.WriteLine("  2. You're using the wrong API endpoint (US vs EU)");
                Console.WriteLine("\nFor EU accounts, update the API URL to: api.eu.mailgun.net");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.ResetColor();

            if (ex.InnerException != null)
            {
                Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
        }
        finally
        {
            serviceProvider?.Dispose();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}