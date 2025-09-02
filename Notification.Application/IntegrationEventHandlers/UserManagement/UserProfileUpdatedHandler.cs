using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.UserManagement.Domain.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.UserManagement;

/// <summary>
/// Handles UserProfileUpdatedIntegrationEvent for profile update notifications.
/// </summary>
public class UserProfileUpdatedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    ILogger<UserProfileUpdatedHandler> logger) : NotificationEventHandler<UserProfileUpdatedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(UserProfileUpdatedIntegrationEvent notification)
    {
        return (notification.UserId, "email.account.changes");
    }

    protected override async Task ProcessNotificationAsync(
        UserProfileUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Only send notification for significant changes
        if (!HasSignificantChanges(notification.Changes))
        {
            return;
        }

        // Get email from changes or use a default notification approach
        var email = notification.Changes.ContainsKey("Email")
            ? notification.Changes["Email"]?.ToString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrEmpty(email))
        {
            // Cannot send email without recipient
            return;
        }

        // Build change summary
        var changesSummary = BuildChangesSummary(notification.Changes);

        var emailBody = $@"
            <h2>Actualización de Perfil</h2>
            <p>Tu perfil ha sido actualizado con los siguientes cambios:</p>
            <ul>
                {changesSummary}
            </ul>
            <p>Si no realizaste estos cambios, por favor contacta al administrador del sistema inmediatamente.</p>
            <br/>
            <p>Saludos,<br/>El equipo de LinaSys</p>";

        emailQueueService.QueueEmail(
            email,
            "Perfil Actualizado",
            emailBody);

        await Task.CompletedTask; // Keep async signature for consistency
    }

    private bool HasSignificantChanges(Dictionary<string, object> changes)
    {
        // Define which changes are significant enough to warrant an email
        var significantFields = new[] { "Email", "FirstName", "LastName", "PhoneNumber", "Identification" };
        return changes.Keys.Any(key => significantFields.Contains(key, StringComparer.OrdinalIgnoreCase));
    }

    private string BuildChangesSummary(Dictionary<string, object> changes)
    {
        var summary = string.Empty;
        var fieldNames = new Dictionary<string, string>
        {
            ["Email"] = "Correo electrónico",
            ["FirstName"] = "Nombre",
            ["LastName"] = "Apellido",
            ["PhoneNumber"] = "Teléfono",
            ["Identification"] = "Identificación",
            ["Address"] = "Dirección",
            ["City"] = "Ciudad",
            ["State"] = "Estado/Provincia",
            ["Country"] = "País"
        };

        foreach (var change in changes)
        {
            var fieldName = fieldNames.ContainsKey(change.Key) ? fieldNames[change.Key] : change.Key;
            summary += $"<li>{fieldName}: {change.Value}</li>";
        }

        return summary;
    }
}