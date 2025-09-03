using LinaSys.Core.Domain.Aggregates.Dashboard;

namespace LinaSys.Core.Domain.Repositories;

public interface IWidgetRepository
{
    Task<List<DashboardWidget>> GetByRoleAsync(string role);
    Task<List<UserWidgetConfiguration>> GetUserConfigurationsAsync(string userId);
    Task SaveUserConfigurationAsync(UserWidgetConfiguration configuration);
    Task<DashboardWidget?> GetByCodeAsync(string code);
    Task<List<DashboardWidget>> GetAllAsync();
}
