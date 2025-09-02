using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Infrastructure.Workers;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Infrastructure.Services;

public class EmailQueueService(ILogger<EmailQueueService> logger, ITimeProvider timeProvider) : IEmailQueueService
{
    private readonly ConcurrentQueue<Domain.AggregatesModel.Email> _emailQueue = new();

    public void QueueEmail(string to, string subject, string body)
    {
        var email = Domain.AggregatesModel.Email.Create(to, subject, body, timeProvider.UtcNow);

        _emailQueue.Enqueue(email);
        logger.LogInformation("Email queued for {Recipient}", to);

        EmailSenderWorker.NotifyNewEmail(); // Immediately notify the worker
    }

    public bool TryDequeue([NotNullWhen(true)] out Domain.AggregatesModel.Email? email)
    {
        return _emailQueue.TryDequeue(out email);
    }
}
