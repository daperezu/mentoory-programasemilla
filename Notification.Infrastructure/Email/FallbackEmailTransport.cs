using LinaSys.Notification.Domain.Email;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Infrastructure.Email;

public sealed class FallbackEmailTransport(
    IEmailTransport primary,
    IEmailTransport fallback,
    ILogger<FallbackEmailTransport> logger) : IEmailTransport
{
    public async Task SendAsync(EmailEnvelope email, CancellationToken ct = default)
    {
        try
        {
            await primary.SendAsync(email, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Primary transport failed; falling back to secondary for {Recipient}", email.To);
            await fallback.SendAsync(email, ct);
        }
    }
}
