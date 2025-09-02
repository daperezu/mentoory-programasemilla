using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Dashboard;

/// <summary>
/// Notification type enumeration.
/// </summary>
public enum NotificationType
{
    System,
    Task,
    Form,
    Message,
    Achievement,
    Reminder,
    Alert,
    Announcement
}

/// <summary>
/// Notification category enumeration.
/// </summary>
public enum NotificationCategory
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Notification priority enumeration.
/// </summary>
public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// Represents a user notification.
/// </summary>
public class UserNotification : Entity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotification"/> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="category">The notification category.</param>
    /// <param name="priority">The notification priority.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="createdBy">The user who created the notification.</param>
    public UserNotification(
        string userId,
        NotificationType type,
        NotificationCategory category,
        NotificationPriority priority,
        string title,
        string message,
        string? createdBy = null)
        : this()
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Type = type;
        Category = category;
        Priority = priority;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotification"/> class.
    /// </summary>
    protected UserNotification()
    {
        UserId = string.Empty;
        Title = string.Empty;
        Message = string.Empty;
        Type = NotificationType.System;
        Category = NotificationCategory.Info;
        Priority = NotificationPriority.Normal;
        IsRead = false;
        IsDismissed = false;
        IsActionTaken = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the notification type.
    /// </summary>
    public NotificationType Type { get; private set; }

    /// <summary>
    /// Gets the notification category.
    /// </summary>
    public NotificationCategory Category { get; private set; }

    /// <summary>
    /// Gets the notification priority.
    /// </summary>
    public NotificationPriority Priority { get; private set; }

    /// <summary>
    /// Gets the notification title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the notification message.
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Gets the additional data (JSON).
    /// </summary>
    public string? Data { get; private set; }

    /// <summary>
    /// Gets the action URL.
    /// </summary>
    public string? ActionUrl { get; private set; }

    /// <summary>
    /// Gets the action text.
    /// </summary>
    public string? ActionText { get; private set; }

    /// <summary>
    /// Gets the icon class.
    /// </summary>
    public string? IconClass { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the notification is read.
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the notification is dismissed.
    /// </summary>
    public bool IsDismissed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether action is taken.
    /// </summary>
    public bool IsActionTaken { get; private set; }

    /// <summary>
    /// Gets the expiration date.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the read timestamp.
    /// </summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>
    /// Gets the dismissed timestamp.
    /// </summary>
    public DateTime? DismissedAt { get; private set; }

    /// <summary>
    /// Gets the action taken timestamp.
    /// </summary>
    public DateTime? ActionTakenAt { get; private set; }

    /// <summary>
    /// Gets the created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the created by user ID.
    /// </summary>
    public string? CreatedBy { get; private set; }

    /// <summary>
    /// Set additional data.
    /// </summary>
    /// <param name="data">The additional data to set.</param>
    public void SetData(string? data)
    {
        Data = data;
    }

    /// <summary>
    /// Set action.
    /// </summary>
    /// <param name="url">The action URL.</param>
    /// <param name="text">The action text.</param>
    public void SetAction(string? url, string? text)
    {
        ActionUrl = url;
        ActionText = text;
    }

    /// <summary>
    /// Set icon.
    /// </summary>
    /// <param name="iconClass">The icon class to set.</param>
    public void SetIcon(string? iconClass)
    {
        IconClass = iconClass;
    }

    /// <summary>
    /// Set expiration.
    /// </summary>
    /// <param name="expiresAt">The expiration date.</param>
    public void SetExpiration(DateTime? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
        {
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));
        }

        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Mark as read.
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as unread.
    /// </summary>
    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }

    /// <summary>
    /// Dismiss notification.
    /// </summary>
    public void Dismiss()
    {
        if (IsDismissed)
        {
            return;
        }

        IsDismissed = true;
        DismissedAt = DateTime.UtcNow;

        // Dismissing also marks as read
        if (!IsRead)
        {
            MarkAsRead();
        }
    }

    /// <summary>
    /// Mark action as taken.
    /// </summary>
    public void MarkActionTaken()
    {
        if (IsActionTaken)
        {
            return;
        }

        IsActionTaken = true;
        ActionTakenAt = DateTime.UtcNow;

        // Taking action also marks as read
        if (!IsRead)
        {
            MarkAsRead();
        }
    }

    /// <summary>
    /// Check if notification is expired.
    /// </summary>
    /// <returns>True if the notification is expired; otherwise, false.</returns>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
    }

    /// <summary>
    /// Check if notification is active.
    /// </summary>
    /// <returns>True if the notification is active; otherwise, false.</returns>
    public bool IsActive()
    {
        return !IsDismissed && !IsExpired();
    }
}