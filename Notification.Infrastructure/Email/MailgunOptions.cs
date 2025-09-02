namespace LinaSys.Notification.Infrastructure.Email;

public sealed class MailgunOptions
{
    public const string SectionName = "Mailgun";
    public string ApiKey { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;          // e.g. "mg.example.com"
                                                                 // store in Key Vault
    public string FromAddress { get; init; } = string.Empty;      // e.g. "no-reply@example.com"
    public string FromName { get; init; } = string.Empty;         // e.g. "LinaSys"
    public bool TestMode { get; init; } = false;                  // Mailgun "o:testmode"
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
}
