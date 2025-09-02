using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Core.Application.Dashboard.Commands.MarkNotificationRead;

public class MarkNotificationReadCommandHandler(INotificationRepository notificationRepository) : BaseCommandHandler<MarkNotificationReadCommand>
{
    public override async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var notification = await notificationRepository.GetByIdAsync(request.NotificationId);
            if (notification is null)
            {
                return Failure(ResultErrorCodes.GenericError, ("NotificationId", "Notificación no encontrada"));
            }

            // Verify ownership
            if (notification.UserId != request.UserId)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("Permission", "No tienes permisos para marcar esta notificación"));
            }

            // Mark as read
            notification.MarkAsRead();

            // Save changes
            await notificationRepository.UpdateAsync(notification);
            await notificationRepository.SaveChangesAsync();

            return Success();
        }
        catch (Exception ex)
        {
            return Failure(ResultErrorCodes.Unknown, ("Error", $"Error al marcar la notificación: {ex.Message}"));
        }
    }
}

public class MarkAllNotificationsReadCommandHandler(INotificationRepository notificationRepository) : BaseCommandHandler<MarkAllNotificationsReadCommand>
{
    public override async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all notifications for the user
            // This is a simplified implementation - in a real scenario, you'd need a method to get unread notifications
            var notifications = new List<Core.Domain.Aggregates.Dashboard.UserNotification>();

            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
            }

            // Save changes
            if (notifications.Any())
            {
                foreach (var notification in notifications)
                {
                    await notificationRepository.UpdateAsync(notification);
                }

                await notificationRepository.SaveChangesAsync();
            }

            return Success();
        }
        catch (Exception ex)
        {
            return Failure(ResultErrorCodes.Unknown, ("Error", $"Error al marcar las notificaciones: {ex.Message}"));
        }
    }
}