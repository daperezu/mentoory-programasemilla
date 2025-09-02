using LinaSys.Core.Application.Dashboard.Mappings;
using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Auth;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.Core.Application.Dashboard.Queries.GetDashboard;

public class GetDashboardQueryHandler(
    IDashboardRepository dashboardRepository,
    IWidgetRepository widgetRepository,
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService,
    ITimeProvider timeProvider)
    : BaseCommandHandler<GetDashboardQuery, DashboardDto>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public override async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        // Get or create dashboard
        var dashboard = await dashboardRepository.GetByUserAndRoleAsync(request.UserId, request.Role);

        if (dashboard is null)
        {
            // Create default dashboard based on role template
            dashboard = await CreateDefaultDashboardAsync(request.UserId, request.Role);
        }

        // Get widgets for this role
        var widgets = await widgetRepository.GetByRoleAsync(request.Role);

        // Get user's widget configurations
        var userWidgetConfigs = await widgetRepository.GetUserConfigurationsAsync(request.UserId);

        // Get unread notifications
        var notifications = await notificationRepository.GetUnreadByUserAsync(request.UserId);

        // Map to DTO
        var dto = dashboard.ToDto();
        dto.Widgets = widgets.ToDto();
        dto.Notifications = notifications.ToDto();

        // Apply user-specific widget configurations
        foreach (var widget in dto.Widgets)
        {
            var userConfig = userWidgetConfigs.FirstOrDefault(c => c.WidgetId == widget.Id);
            if (userConfig is not null)
            {
                widget.Position = userConfig.Position;
                widget.IsVisible = userConfig.IsVisible;
                widget.Configuration = userConfig.Configuration;
                widget.Width = userConfig.Width;
                widget.Height = userConfig.Height;
            }
        }

        // Sort widgets by position
        dto.Widgets = dto.Widgets.OrderBy(w => w.Position).ToList();

        return Success(dto);
    }

    private async Task<BaseDashboard> CreateDefaultDashboardAsync(string userId, string role)
    {
        // Get role template
        var template = await dashboardRepository.GetRoleTemplateAsync(role);

        // Create new dashboard based on template
        var dashboard = new UserDashboard(userId, role, timeProvider.UtcNow);

        if (template is not null)
        {
            dashboard.ApplyTemplate(template);
        }

        await dashboardRepository.AddAsync(dashboard);
        await dashboardRepository.SaveChangesAsync();

        return dashboard;
    }
}
