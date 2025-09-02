using LinaSys.Notification.Application.Interfaces;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.Commands;

public record SendEmailCommand(string To, string Subject, string Body) : IBaseRequest;

public class SendEmailCommandHandler(IEmailQueueService emailSender, ILogger<SendEmailCommandHandler> logger) : BaseCommandHandler<SendEmailCommand>
{
    public override Task<Result> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Queuing email to {Recipient}", request.To);
        emailSender.QueueEmail(request.To, request.Subject, request.Body);
        return Task.FromResult(Success());
    }
}
