namespace LinaSys.Notification.Domain.AggregatesModel;

public class Email
{
    public required string To { get; set; }

    public required string Subject { get; set; }

    public required string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Creates a new email with the specified timestamp.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>A new Email instance.</returns>
    public static Email Create(string to, string subject, string body, DateTime createdAt)
    {
        return new Email
        {
            To = to,
            Subject = subject,
            Body = body,
            CreatedAt = createdAt
        };
    }
}
