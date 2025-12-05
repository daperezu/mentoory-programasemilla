using LinaSys.Notification.Application.Services;
using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers;

/// <summary>
/// Base class for all notification event handlers with built-in preference checking.
/// </summary>
/// <typeparam name="TEvent">The type of integration event to handle.</typeparam>
public abstract class NotificationEventHandler<TEvent>(
    IEmailPreferenceService emailPreferenceService,
    ILogger logger) : INotificationHandler<TEvent>
    where TEvent : IIntegrationEvent
{
    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Extract user ID and preference key from the event
            var (userId, preferenceKey) = GetUserAndPreferenceKey(notification);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(preferenceKey))
            {
                logger.LogWarning("Cannot process event {EventType}: missing user ID or preference key", typeof(TEvent).Name);
                return;
            }

            // Check if user wants this type of email
            var canSend = await emailPreferenceService.CanSendEmailAsync(userId, preferenceKey, cancellationToken);

            if (!canSend)
            {
                logger.LogInformation(
                    "Email not sent for event {EventType} to user {UserId}: preference {PreferenceKey} is disabled",
                    typeof(TEvent).Name, userId, preferenceKey);
                return;
            }

            // Process the notification
            await ProcessNotificationAsync(notification, cancellationToken);

            logger.LogInformation(
                "Successfully processed notification for event {EventType} to user {UserId}",
                typeof(TEvent).Name, userId);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - we don't want to fail the main business operation
            logger.LogError(ex,
                "Failed to process notification for event {EventType} with ID {EventId}",
                typeof(TEvent).Name, notification.EventId);
        }
    }

    /// <summary>
    /// Extract the user ID and preference key from the event.
    /// </summary>
    /// <returns></returns>
    protected abstract (string UserId, string PreferenceKey) GetUserAndPreferenceKey(TEvent notification);

    /// <summary>
    /// Process the notification after preference check passes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected abstract Task ProcessNotificationAsync(TEvent notification, CancellationToken cancellationToken);
}