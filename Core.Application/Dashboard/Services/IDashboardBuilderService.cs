using LinaSys.Core.Application.Dashboard.Queries.GetDashboard;

namespace LinaSys.Core.Application.Dashboard.Services;

public interface IDashboardBuilderService
{
    Task<DashboardDto> BuildDashboardAsync(string userId, string role);
    Task<DashboardDto> BuildStarterDashboardAsync(string userId, long projectId);
    Task<DashboardDto> BuildMentorDashboardAsync(string userId);
    Task<DashboardDto> BuildCoordinatorDashboardAsync(string userId);
    Task<DashboardDto> BuildAdminDashboardAsync(string userId);
    Task<object> LoadWidgetDataAsync(string widgetCode, string userId, Dictionary<string, object>? parameters = null);
}
