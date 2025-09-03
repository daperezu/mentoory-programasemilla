using LinaSys.Core.Application.Dashboard.Mappings;
using LinaSys.Core.Application.Dashboard.Queries.GetDashboard;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Application.Auth;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace LinaSys.Core.Infrastructure.Services;

public class DashboardBuilderService(
    IDashboardRepository dashboardRepository,
    IWidgetRepository widgetRepository,
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService,
    ITimeProvider timeProvider,
    ILogger<DashboardBuilderService> logger) : IDashboardBuilderService
{
    public async Task<DashboardDto> BuildDashboardAsync(string userId, string role)
    {
        logger.LogInformation("Building dashboard for user {UserId} with role {Role}", userId, role);

        // Delegate to specific dashboard builders based on role
        return role switch
        {
            Roles.Starter => await BuildStarterDashboardAsync(userId, 0), // ProjectId would come from context
            Roles.Mentor => await BuildMentorDashboardAsync(userId),
            Roles.Coordinator => await BuildCoordinatorDashboardAsync(userId),
            Roles.Administrator => await BuildAdminDashboardAsync(userId),
            _ => await BuildDefaultDashboardAsync(userId, role)
        };
    }

    public async Task<DashboardDto> BuildStarterDashboardAsync(string userId, long projectId)
    {
        var dashboard = await GetOrCreateDashboard(userId, "starter");
        var widgets = await widgetRepository.GetByRoleAsync("starter");
        var notifications = await notificationRepository.GetUnreadByUserAsync(userId);

        var dto = new DashboardDto
        {
            UserId = userId,
            UserName = currentUserService.UserName ?? "Usuario",
            Role = Roles.Starter,
            RoleName = "Emprendedor",
            Layout = dashboard.Layout ?? "default",
            Theme = dashboard.Theme,
            Language = dashboard.Language,
            Widgets = widgets.ToDto(),
            Notifications = notifications.ToDto(),
            Preferences = dashboard.Preferences?.ToDto() ?? new DashboardPreferencesDto(),
            LastActivityDate = dashboard.LastActivityDate ?? DateTime.UtcNow,
            IsFirstLogin = dashboard.CreatedDate.Date == DateTime.UtcNow.Date
        };

        // Load widget data
        foreach (var widget in dto.Widgets)
        {
            widget.Data = await LoadWidgetDataAsync(widget.Code, userId, new Dictionary<string, object> { { "projectId", projectId } });
        }

        return dto;
    }

    public async Task<DashboardDto> BuildMentorDashboardAsync(string userId)
    {
        var dashboard = await GetOrCreateDashboard(userId, "mentor");
        var widgets = await widgetRepository.GetByRoleAsync("mentor");
        var notifications = await notificationRepository.GetUnreadByUserAsync(userId);

        var dto = new DashboardDto
        {
            UserId = userId,
            UserName = currentUserService.UserName ?? "Usuario",
            Role = Roles.Mentor,
            RoleName = "Mentor",
            Layout = dashboard.Layout ?? "default",
            Theme = dashboard.Theme,
            Language = dashboard.Language,
            Widgets = widgets.ToDto(),
            Notifications = notifications.ToDto(),
            Preferences = dashboard.Preferences?.ToDto() ?? new DashboardPreferencesDto(),
            LastActivityDate = dashboard.LastActivityDate ?? DateTime.UtcNow,
            IsFirstLogin = dashboard.CreatedDate.Date == DateTime.UtcNow.Date
        };

        return dto;
    }

    public async Task<DashboardDto> BuildCoordinatorDashboardAsync(string userId)
    {
        var dashboard = await GetOrCreateDashboard(userId, "coordinator");
        var widgets = await widgetRepository.GetByRoleAsync("coordinator");
        var notifications = await notificationRepository.GetUnreadByUserAsync(userId);

        var dto = new DashboardDto
        {
            UserId = userId,
            UserName = currentUserService.UserName ?? "Usuario",
            Role = Roles.Coordinator,
            RoleName = "Coordinador",
            Layout = dashboard.Layout ?? "default",
            Theme = dashboard.Theme,
            Language = dashboard.Language,
            Widgets = widgets.ToDto(),
            Notifications = notifications.ToDto(),
            Preferences = dashboard.Preferences?.ToDto() ?? new DashboardPreferencesDto(),
            LastActivityDate = dashboard.LastActivityDate ?? DateTime.UtcNow,
            IsFirstLogin = dashboard.CreatedDate.Date == DateTime.UtcNow.Date
        };

        return dto;
    }

    public async Task<DashboardDto> BuildAdminDashboardAsync(string userId)
    {
        var dashboard = await GetOrCreateDashboard(userId, "administrator");
        var widgets = await widgetRepository.GetByRoleAsync("administrator");
        var notifications = await notificationRepository.GetUnreadByUserAsync(userId);

        var dto = new DashboardDto
        {
            UserId = userId,
            UserName = currentUserService.UserName ?? "Usuario",
            Role = Roles.Administrator,
            RoleName = "Administrador",
            Layout = dashboard.Layout ?? "default",
            Theme = dashboard.Theme,
            Language = dashboard.Language,
            Widgets = widgets.ToDto(),
            Notifications = notifications.ToDto(),
            Preferences = dashboard.Preferences?.ToDto() ?? new DashboardPreferencesDto(),
            LastActivityDate = dashboard.LastActivityDate ?? DateTime.UtcNow,
            IsFirstLogin = dashboard.CreatedDate.Date == DateTime.UtcNow.Date
        };

        return dto;
    }

    public Task<object> LoadWidgetDataAsync(string widgetCode, string userId, Dictionary<string, object>? parameters = null)
    {
        logger.LogDebug("Loading data for widget {WidgetCode}", widgetCode);

        // Widget-specific data loading logic
        var result = widgetCode switch
        {
            "progress_overview" => (object)new { progress = 65, phase = "Diagnóstico" },
            "pending_tasks" => (object)new { count = 5, urgent = 2 },
            "recent_activities" => (object)new { activities = new[] { "Formulario enviado", "Tarea completada" } },
            "notifications" => (object)new { unread = 3, total = 10 },
            _ => (object)new { message = "Widget data not implemented" }
        };

        return Task.FromResult<object>(result);
    }

    private async Task<DashboardDto> BuildDefaultDashboardAsync(string userId, string role)
    {
        var dashboard = await GetOrCreateDashboard(userId, role);
        var widgets = await widgetRepository.GetByRoleAsync(role);
        var notifications = await notificationRepository.GetUnreadByUserAsync(userId);

        return new DashboardDto
        {
            UserId = userId,
            UserName = currentUserService.UserName ?? "Usuario",
            Role = role,
            RoleName = role,
            Layout = dashboard.Layout ?? "default",
            Theme = dashboard.Theme,
            Language = dashboard.Language,
            Widgets = widgets.ToDto(),
            Notifications = notifications.ToDto(),
            Preferences = dashboard.Preferences?.ToDto() ?? new DashboardPreferencesDto(),
            LastActivityDate = dashboard.LastActivityDate ?? DateTime.UtcNow,
            IsFirstLogin = dashboard.CreatedDate.Date == DateTime.UtcNow.Date
        };
    }

    private async Task<Domain.Aggregates.Dashboard.BaseDashboard> GetOrCreateDashboard(string userId, string role)
    {
        var dashboard = await dashboardRepository.GetByUserAndRoleAsync(userId, role);

        if (dashboard is null)
        {
            var userDashboard = new Domain.Aggregates.Dashboard.UserDashboard(userId, role, timeProvider.UtcNow);

            // Apply role template if available
            var template = await dashboardRepository.GetRoleTemplateAsync(role);
            if (template is not null)
            {
                userDashboard.ApplyTemplate(template);
            }

            await dashboardRepository.AddAsync(userDashboard);
            await dashboardRepository.SaveChangesAsync();

            dashboard = userDashboard;
        }

        return dashboard;
    }
}
