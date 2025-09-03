using LinaSys.Notification.Domain.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LinaSys.Notification.Infrastructure.Email;

public sealed class SmtpEmailTransport(IOptions<MailSmtpOptions> options, ILogger<SmtpEmailTransport> logger) : IEmailTransport
{
    private readonly MailSmtpOptions _opt = options.Value;

    public async Task SendAsync(EmailEnvelope email, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
        message.To.Add(MailboxAddress.Parse(email.To));
        message.Subject = email.Subject;

        if (!string.IsNullOrWhiteSpace(email.TextBody))
        {
            var alternative = new MultipartAlternative
            {
                new TextPart("plain") { Text = email.TextBody! },
                new TextPart("html") { Text = email.HtmlBody }
            };
            message.Body = alternative;
        }
        else
        {
            message.Body = new TextPart("html") { Text = email.HtmlBody };
        }

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _opt.SmtpServer,
            _opt.SmtpPort,
            _opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
            ct);

        if (!string.IsNullOrEmpty(_opt.Username))
        {
            await client.AuthenticateAsync(_opt.Username, _opt.Password, ct);
        }

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("SMTP email sent to {Recipient}", email.To);
    }
}
