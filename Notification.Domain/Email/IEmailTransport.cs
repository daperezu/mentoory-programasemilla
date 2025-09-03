namespace LinaSys.Notification.Domain.Email;

public interface IEmailTransport
{
    Task SendAsync(EmailEnvelope email, CancellationToken ct = default);
}
