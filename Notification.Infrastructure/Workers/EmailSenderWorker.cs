using LinaSys.Notification.Domain.Email;
using LinaSys.Notification.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Infrastructure.Workers;

public class EmailSenderWorker(ILogger<EmailSenderWorker> logger, EmailQueueService emailQueueService, IEmailTransport emailTransport) : BackgroundService
{
    private static readonly SemaphoreSlim _signal = new(0);

    public static void NotifyNewEmail()
    {
        _signal.Release(); // Wake up the worker when an email is queued
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Email sender worker started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(cancellationToken); // Wait until an email is queued

            while (emailQueueService.TryDequeue(out var email))
            {
                try
                {
                    await emailTransport.SendAsync(new EmailEnvelope(email.To, email.Subject, email.Body), cancellationToken);
                    logger.LogInformation("Email sent to {Recipient}", email.To);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send email to {Recipient}", email.To);
                }
            }
        }

        logger.LogInformation("Email sender worker ended.");
    }
}
