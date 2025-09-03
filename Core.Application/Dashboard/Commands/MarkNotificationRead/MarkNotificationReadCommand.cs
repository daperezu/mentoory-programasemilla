using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Dashboard.Commands.MarkNotificationRead;

public class MarkNotificationReadCommand(string userId, long notificationId) : IBaseRequest
{
    public string UserId { get; } = userId;
    public long NotificationId { get; } = notificationId;
}

public class MarkAllNotificationsReadCommand(string userId) : IBaseRequest
{
    public string UserId { get; } = userId;
}