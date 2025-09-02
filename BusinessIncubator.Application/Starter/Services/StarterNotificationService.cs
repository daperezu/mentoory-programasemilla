using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.BusinessIncubator.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Starter.Services;

public class StarterNotificationService(
    IStarterRepository starterRepository,
    ILogger<StarterNotificationService> logger) : IStarterNotificationService
{

    // Notification templates
    private readonly Dictionary<string, (string Title, string Message)> _templates = new()
    {
        ["task_reminder"] = (
            "Recordatorio de Tarea",
            "La tarea '{0}' está programada para hoy. No olvides completarla."),
        ["milestone_completed"] = (
            "¡Hito Alcanzado!",
            "¡Felicidades! Has completado el hito '{0}'. Sigue así con tu progreso."),
        ["phase_completed"] = (
            "Fase Completada",
            "¡Excelente! Has completado la fase de {0}. Tu proyecto avanza muy bien."),
        ["mentor_assigned"] = (
            "Mentor Asignado",
            "Se te ha asignado a {0} como mentor. Pronto recibirás información para tu primera sesión."),
        ["meeting_reminder"] = (
            "Recordatorio de Reunión",
            "Tienes una reunión programada para {0}. Prepara tus preguntas y documentación."),
        ["progress_update"] = (
            "Actualización de Progreso",
            "Tu progreso actual es del {0}%. ¡Sigue trabajando para alcanzar tus objetivos!"),
        ["overdue_alert"] = (
            "Tareas Vencidas",
            "Tienes {0} tareas vencidas. Por favor, revísalas y actualiza su estado.")
    };

    public async Task SendTaskReminderAsync(string userId, long taskId)
    {
        var task = await starterRepository.GetTaskByIdAsync(taskId);
        if (task is null)
        {
            logger.LogWarning("Task {TaskId} not found for reminder", taskId);
            return;
        }

        var (title, messageTemplate) = _templates["task_reminder"];
        var message = string.Format(messageTemplate, task.Title);

        await CreateNotificationAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "info",
            Category = "task",
            Priority = DeterminePriority(task.Priority),
            ActionUrl = $"/BusinessIncubators/Starter/Task/{taskId}",
            ActionText = "Ver Tarea",
            CreatedAt = DateTime.UtcNow
        });

        logger.LogInformation("Sent task reminder for task {TaskId} to user {UserId}", taskId, userId);
    }

    public async Task SendMilestoneCompletedAsync(string userId, long projectId, string milestoneName)
    {
        var (title, messageTemplate) = _templates["milestone_completed"];
        var message = string.Format(messageTemplate, milestoneName);

        await CreateNotificationAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "success",
            Category = "milestone",
            Priority = "high",
            ActionUrl = $"/BusinessIncubators/Starter/Progress?projectId={projectId}",
            ActionText = "Ver Progreso",
            CreatedAt = DateTime.UtcNow
        });

        logger.LogInformation("Sent milestone completed notification for {Milestone} to user {UserId}",
            milestoneName, userId);
    }

    public async Task SendPhaseCompletedAsync(string userId, long projectId, string phase)
    {
        var (title, messageTemplate) = _templates["phase_completed"];
        var phaseName = GetPhaseDisplayName(phase);
        var message = string.Format(messageTemplate, phaseName);

        await CreateNotificationAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "success",
            Category = "progress",
            Priority = "high",
            ActionUrl = $"/BusinessIncubators/Starter/Dashboard?projectId={projectId}",
            ActionText = "Ver Dashboard",
            CreatedAt = DateTime.UtcNow
        });

        // Generate tasks for the next phase
        var nextPhase = GetNextPhase(phase);
        if (!string.IsNullOrEmpty(nextPhase))
        {
            await CreateNotificationAsync(new UserNotification
            {
                UserId = userId,
                Title = "Nueva Fase Iniciada",
                Message = $"Has iniciado la fase de {GetPhaseDisplayName(nextPhase)}. Se han generado nuevas tareas.",
                Type = "info",
                Category = "progress",
                Priority = "normal",
                ActionUrl = $"/BusinessIncubators/Starter/Tasks?projectId={projectId}",
                ActionText = "Ver Tareas",
                CreatedAt = DateTime.UtcNow
            });
        }

        logger.LogInformation("Sent phase completed notification for {Phase} to user {UserId}", phase, userId);
    }

    public async Task SendMentorAssignedAsync(string userId, long projectId, string mentorName)
    {
        var (title, messageTemplate) = _templates["mentor_assigned"];
        var message = string.Format(messageTemplate, mentorName);

        await CreateNotificationAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "info",
            Category = "meeting",
            Priority = "normal",
            ActionUrl = $"/BusinessIncubators/Starter/Mentor?projectId={projectId}",
            ActionText = "Ver Mentor",
            CreatedAt = DateTime.UtcNow
        });

        logger.LogInformation("Sent mentor assigned notification for {MentorName} to user {UserId}",
            mentorName, userId);
    }

    public async Task SendMeetingReminderAsync(string userId, long projectId, DateTime meetingDate)
    {
        var (title, messageTemplate) = _templates["meeting_reminder"];
        var formattedDate = meetingDate.ToString("dd/MM/yyyy HH:mm");
        var message = string.Format(messageTemplate, formattedDate);

        await CreateNotificationAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "warning",
            Category = "meeting",
            Priority = "high",
            ActionUrl = $"/BusinessIncubators/Meetings/Join?projectId={projectId}",
            ActionText = "Unirse a Reunión",
            CreatedAt = DateTime.UtcNow
        });

        logger.LogInformation("Sent meeting reminder for {MeetingDate} to user {UserId}",
            meetingDate, userId);
    }

    public async Task SendProgressUpdateAsync(string userId, long projectId, decimal progress)
    {
        var (title, messageTemplate) = _templates["progress_update"];
        var message = string.Format(messageTemplate, progress.ToString("F1"));

        // Determine notification type based on progress
        var type = progress switch
        {
            >= 80 => "success",
            >= 50 => "info",
            >= 25 => "warning",
            _ => "error"
        };

        await CreateNotificationAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Category = "progress",
            Priority = "normal",
            ActionUrl = $"/BusinessIncubators/Starter/Dashboard?projectId={projectId}",
            ActionText = "Ver Dashboard",
            CreatedAt = DateTime.UtcNow
        });

        logger.LogInformation("Sent progress update ({Progress}%) to user {UserId}", progress, userId);
    }

    public async Task SendOverdueTasksAlertAsync(string userId, long projectId, int overdueCount)
    {
        var (title, messageTemplate) = _templates["overdue_alert"];
        var message = string.Format(messageTemplate, overdueCount);

        await CreateNotificationAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "error",
            Category = "task",
            Priority = "urgent",
            ActionUrl = $"/BusinessIncubators/Starter/Tasks?projectId={projectId}&filter=overdue",
            ActionText = "Ver Tareas Vencidas",
            CreatedAt = DateTime.UtcNow
        });

        logger.LogInformation("Sent overdue tasks alert ({Count} tasks) to user {UserId}",
            overdueCount, userId);
    }

    public async Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int count = 10)
    {
        // TODO: Implement through a proper notification repository
        // For now, return empty list
        logger.LogDebug("Getting notifications for user {UserId}", userId);
        await Task.CompletedTask;
        return [];
    }

    public async Task MarkNotificationAsReadAsync(long notificationId)
    {
        // TODO: Implement through a proper notification repository
        await starterRepository.MarkNotificationAsReadAsync(notificationId);
        logger.LogDebug("Marked notification {NotificationId} as read", notificationId);
    }

    public async Task DismissNotificationAsync(long notificationId)
    {
        // TODO: Implement through a proper notification repository
        // For now, just mark as read
        await starterRepository.MarkNotificationAsReadAsync(notificationId);
        logger.LogDebug("Dismissed notification {NotificationId}", notificationId);
    }

    private async Task CreateNotificationAsync(UserNotification notification)
    {
        // TODO: Implement through a proper notification repository
        // For now, just log the notification
        logger.LogInformation("Creating notification '{Title}' for user {UserId}",
            notification.Title, notification.UserId);
        await Task.CompletedTask;
    }

    private string DeterminePriority(string taskPriority)
    {
        return taskPriority?.ToLower() switch
        {
            "urgent" => "urgent",
            "high" => "high",
            "normal" => "normal",
            "low" => "low",
            _ => "normal"
        };
    }

    private string GetPhaseDisplayName(string phase)
    {
        return phase?.ToLower() switch
        {
            "diagnosis" => "Diagnóstico",
            "development" => "Desarrollo",
            "validation" => "Validación",
            "implementation" => "Implementación",
            "growth" => "Crecimiento",
            _ => phase ?? "Desconocida"
        };
    }

    private string? GetNextPhase(string currentPhase)
    {
        return currentPhase?.ToLower() switch
        {
            "diagnosis" => "development",
            "development" => "validation",
            "validation" => "implementation",
            "implementation" => "growth",
            "growth" => null,
            _ => null
        };
    }
}