namespace LinaSys.Notification.Infrastructure.Email;

public sealed class MailSmtpOptions
{
    public const string SectionName = "MailSmtp";
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public string SmtpServer { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public bool UseStartTls { get; init; } = true;
}
