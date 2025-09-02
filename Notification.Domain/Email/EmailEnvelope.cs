namespace LinaSys.Notification.Domain.Email;

public sealed record EmailEnvelope(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null,
    string? Tag = null,
    IDictionary<string, string>? Variables = null);
