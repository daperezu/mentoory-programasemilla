using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.Core.Infrastructure.Services;

public class NotificationService(
    INotificationRepository repository,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<UserNotification> CreateNotificationAsync(
        string userId,
        string title,
        string message,
        string type,
        string priority = "normal",
        string category = "general",
        string? actionUrl = null,
        string? actionText = null)
    {
        // Parse string parameters to enum values
        var notificationType = Enum.TryParse<NotificationType>(type, true, out var parsedType) ? parsedType : NotificationType.System;
        var notificationCategory = Enum.TryParse<NotificationCategory>(category, true, out var parsedCategory) ? parsedCategory : NotificationCategory.Info;
        var notificationPriority = Enum.TryParse<NotificationPriority>(priority, true, out var parsedPriority) ? parsedPriority : NotificationPriority.Normal;

        var notification = new UserNotification(
            userId,
            notificationType,
            notificationCategory,
            notificationPriority,
            title,
            message);

        if (!string.IsNullOrEmpty(actionUrl))
        {
            notification.SetAction(actionUrl, actionText);
        }

        await repository.AddAsync(notification);
        await repository.SaveChangesAsync();

        logger.LogInformation("Notification created for user {UserId}: {Title}", userId, title);

        return notification;
    }

    public async Task<List<UserNotification>> GetUnreadNotificationsAsync(string userId)
    {
        return await repository.GetUnreadByUserAsync(userId);
    }

    public async Task<List<UserNotification>> GetRecentNotificationsAsync(string userId, int count = 10)
    {
        return await repository.GetRecentByUserAsync(userId, count);
    }

    public async Task MarkAsReadAsync(long notificationId)
    {
        var notification = await repository.GetByIdAsync(notificationId);
        if (notification is not null)
        {
            notification.MarkAsRead();
            await repository.UpdateAsync(notification);
            await repository.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await repository.GetUnreadByUserAsync(userId);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
            await repository.UpdateAsync(notification);
        }

        if (unreadNotifications.Count > 0)
        {
            await repository.SaveChangesAsync();
            logger.LogInformation("Marked {Count} notifications as read for user {UserId}",
                unreadNotifications.Count, userId);
        }
    }

    public async Task DeleteNotificationAsync(long notificationId)
    {
        await repository.DeleteAsync(notificationId);
        await repository.SaveChangesAsync();
    }

    public async Task DeleteOldNotificationsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        // This would require a custom repository method to delete old notifications
        // For now, we'll log the intention
        logger.LogInformation("Cleaning up notifications older than {Days} days", daysOld);
        await Task.CompletedTask;
    }

    public async Task SendBulkNotificationAsync(List<string> userIds, string title, string message, string type)
    {
        var tasks = userIds.Select(userId =>
            CreateNotificationAsync(userId, title, message, type));

        await Task.WhenAll(tasks);

        logger.LogInformation("Sent bulk notification to {Count} users: {Title}", userIds.Count, title);
    }
}
