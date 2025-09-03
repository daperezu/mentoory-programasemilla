using System.Security.Claims;
using LinaSys.Core.Application.Activities.Queries;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.ViewComponents;

public class AuditHistoryViewComponent(
    MediatorExecutor mediatorExecutor,
    ILogger<AuditHistoryViewComponent> logger) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        string? entityType = null,
        long? entityId = null,
        string? userId = null,
        int pageSize = 20)
    {
        try
        {
            // If no userId provided, try to get current user
            if (string.IsNullOrEmpty(userId))
            {
                userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("No user ID available for audit history");
                return View(new AuditHistoryViewModel
                {
                    Activities = new List<AuditActivityViewModel>(),
                    EntityType = entityType,
                    EntityId = entityId,
                    HasMore = false
                });
            }

            // Get activities using existing query
            var result = await mediatorExecutor.SendAndLogIfFailureAsync(
                new GetUserActivitiesQuery(userId, pageSize));

            if (!result.IsSuccess || result.Value == null)
            {
                logger.LogWarning("Failed to retrieve audit history for user {UserId}", userId);
                return View(new AuditHistoryViewModel
                {
                    Activities = new List<AuditActivityViewModel>(),
                    EntityType = entityType,
                    EntityId = entityId,
                    HasMore = false
                });
            }

            // Filter by entity if specified
            var activities = result.Value;
            if (!string.IsNullOrEmpty(entityType) && entityId.HasValue)
            {
                activities = activities
                    .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                    .ToList();
            }

            // Convert to view models with metadata parsing
            var viewModels = activities.Select(activity => new AuditActivityViewModel
            {
                Id = activity.Id,
                ActivityType = activity.ActivityType,
                Description = FormatActivityDescription(activity),
                EntityType = activity.EntityType,
                EntityId = activity.EntityId,
                CreatedDate = activity.CreatedDate,
                RelativeTime = GetRelativeTime(activity.CreatedDate),
                Icon = GetActivityIcon(activity.ActivityType),
                Color = GetActivityColor(activity.ActivityType),
                Metadata = ParseMetadata(activity.Metadata),
                Category = GetActivityCategory(activity.ActivityType)
            }).ToList();

            return View(new AuditHistoryViewModel
            {
                Activities = viewModels,
                EntityType = entityType,
                EntityId = entityId,
                HasMore = activities.Count == pageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating audit history view");
            return View(new AuditHistoryViewModel
            {
                Activities = new List<AuditActivityViewModel>(),
                EntityType = entityType,
                EntityId = entityId,
                HasMore = false
            });
        }
    }

    private static string FormatActivityDescription(UserActivityDto activity)
    {
        // Add user-friendly descriptions based on activity type
        return activity.ActivityType.ToLowerInvariant() switch
        {
            "login" => "Inició sesión",
            "logout" => "Cerró sesión",
            "project_joined" => "Se unió al proyecto",
            "form_submitted" => "Envió un formulario",
            "form_approved" => "Formulario aprobado",
            "form_rejected" => "Formulario rechazado",
            "profile_updated" => "Actualizó su perfil",
            "password_changed" => "Cambió su contraseña",
            "email_verified" => "Verificó su correo electrónico",
            "role_assigned" => "Se le asignó un rol",
            "invitation_sent" => "Se envió una invitación",
            "invitation_accepted" => "Aceptó la invitación",
            "user_created" => "Cuenta creada",
            "user_updated" => "Información actualizada",
            "user_deactivated" => "Cuenta desactivada",
            "user_activated" => "Cuenta activada",
            _ => activity.Description
        };
    }

    private static Dictionary<string, string?> ParseMetadata(string? metadata)
    {
        var result = new Dictionary<string, string?>();

        if (string.IsNullOrEmpty(metadata))
        {
            return result;
        }

        try
        {
            // Assuming metadata is JSON, parse it
            var json = System.Text.Json.JsonDocument.Parse(metadata);
            foreach (var property in json.RootElement.EnumerateObject())
            {
                result[property.Name] = property.Value.ToString();
            }
        }
        catch
        {
            // If parsing fails, treat as plain text
            result["raw"] = metadata;
        }

        return result;
    }

    private static string GetActivityCategory(string activityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" or "logout" => "Autenticación",
            "user_created" or "user_updated" or "user_deactivated" or "user_activated" => "Gestión de Usuarios",
            "project_joined" or "role_assigned" => "Operaciones de Proyecto",
            "form_submitted" or "form_approved" or "form_rejected" => "Actividades de Formularios",
            "email_verified" or "invitation_sent" or "invitation_accepted" => "Eventos del Sistema",
            "profile_updated" or "password_changed" => "Cuenta Personal",
            _ => "Otros"
        };
    }

    private static string GetRelativeTime(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
        {
            return "Hace un momento";
        }

        if (timeSpan.TotalMinutes < 60)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return $"Hace {minutes} {(minutes == 1 ? "minuto" : "minutos")}";
        }

        if (timeSpan.TotalHours < 24)
        {
            var hours = (int)timeSpan.TotalHours;
            return $"Hace {hours} {(hours == 1 ? "hora" : "horas")}";
        }

        if (timeSpan.TotalDays < 30)
        {
            var days = (int)timeSpan.TotalDays;
            return $"Hace {days} {(days == 1 ? "día" : "días")}";
        }

        if (timeSpan.TotalDays < 365)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return $"Hace {months} {(months == 1 ? "mes" : "meses")}";
        }

        var years = (int)(timeSpan.TotalDays / 365);
        return $"Hace {years} {(years == 1 ? "año" : "años")}";
    }

    private static string GetActivityIcon(string activityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" => "bi-box-arrow-in-right",
            "logout" => "bi-box-arrow-right",
            "project_joined" => "bi-folder-plus",
            "form_submitted" => "bi-file-earmark-check",
            "form_approved" => "bi-check-circle",
            "form_rejected" => "bi-x-circle",
            "profile_updated" => "bi-person-gear",
            "password_changed" => "bi-key",
            "email_verified" => "bi-envelope-check",
            "role_assigned" => "bi-shield-check",
            "invitation_sent" => "bi-envelope-plus",
            "invitation_accepted" => "bi-person-check",
            _ => "bi-info-circle"
        };
    }

    private static string GetActivityColor(string activityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" or "logout" => "info",
            "project_joined" or "invitation_accepted" => "success",
            "form_submitted" => "primary",
            "form_approved" => "success",
            "form_rejected" => "danger",
            "profile_updated" or "password_changed" => "warning",
            "email_verified" or "role_assigned" => "success",
            "invitation_sent" => "info",
            _ => "secondary"
        };
    }
}

public class AuditHistoryViewModel
{
    public List<AuditActivityViewModel> Activities { get; set; } = new();
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public bool HasMore { get; set; }
}

public class AuditActivityViewModel
{
    public long Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string RelativeTime { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-info-circle";
    public string Color { get; set; } = "primary";
    public Dictionary<string, string?> Metadata { get; set; } = new();
    public string Category { get; set; } = string.Empty;
}
