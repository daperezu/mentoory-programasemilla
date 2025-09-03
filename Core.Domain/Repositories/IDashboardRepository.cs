using LinaSys.Core.Domain.Aggregates.Dashboard;

namespace LinaSys.Core.Domain.Repositories;

public interface IDashboardRepository
{
    Task<BaseDashboard?> GetByIdAsync(long id);
    Task<BaseDashboard?> GetByUserIdAsync(string userId);
    Task<BaseDashboard?> GetByUserAndRoleAsync(string userId, string role);
    Task<RoleDashboardTemplate?> GetRoleTemplateAsync(string role);
    Task AddAsync(BaseDashboard dashboard);
    Task UpdateAsync(BaseDashboard dashboard);
    Task DeleteAsync(long id);
    Task SaveChangesAsync();
}
