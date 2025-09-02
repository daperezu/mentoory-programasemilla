using LinaSys.Core.Domain.Aggregates.Dashboard;

namespace LinaSys.Core.Application.Dashboard.Services;

public interface INotificationService
{
    Task<UserNotification> CreateNotificationAsync(
        string userId,
        string title,
        string message,
        string type,
        string priority = "normal",
        string category = "general",
        string? actionUrl = null,
        string? actionText = null);

    Task<List<UserNotification>> GetUnreadNotificationsAsync(string userId);
    Task<List<UserNotification>> GetRecentNotificationsAsync(string userId, int count = 10);
    Task MarkAsReadAsync(long notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteNotificationAsync(long notificationId);
    Task DeleteOldNotificationsAsync(int daysOld = 30);
    Task SendBulkNotificationAsync(List<string> userIds, string title, string message, string type);
}