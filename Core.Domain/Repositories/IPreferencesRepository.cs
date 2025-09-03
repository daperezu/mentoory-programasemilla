using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Repositories;

public interface IPreferencesRepository : IRepository<UserDashboard>
{
    Task<DashboardPreferences?> GetUserPreferencesAsync(string userId);
    Task SaveUserPreferencesAsync(string userId, DashboardPreferences preferences);
    Task DeleteUserPreferencesAsync(string userId);
}