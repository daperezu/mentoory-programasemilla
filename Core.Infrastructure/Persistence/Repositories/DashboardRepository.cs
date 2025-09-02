using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Core.Infrastructure.Persistence.Repositories;

public class DashboardRepository(CoreDbContext context) : IDashboardRepository
{
    public async Task<BaseDashboard?> GetByIdAsync(long id)
    {
        return await context.UserDashboards
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<BaseDashboard?> GetByUserIdAsync(string userId)
    {
        return await context.UserDashboards
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<BaseDashboard?> GetByUserAndRoleAsync(string userId, string role)
    {
        return await context.UserDashboards
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Role == role);
    }

    public async Task<RoleDashboardTemplate?> GetRoleTemplateAsync(string role)
    {
        return await context.RoleDashboardTemplates
            .FirstOrDefaultAsync(t => t.Role == role && t.IsActive);
    }

    public async Task AddAsync(BaseDashboard dashboard)
    {
        if (dashboard is UserDashboard userDashboard)
        {
            await context.UserDashboards.AddAsync(userDashboard);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported dashboard type: {dashboard.GetType().Name}");
        }
    }

    public async Task UpdateAsync(BaseDashboard dashboard)
    {
        if (dashboard is UserDashboard userDashboard)
        {
            context.UserDashboards.Update(userDashboard);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported dashboard type: {dashboard.GetType().Name}");
        }

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(long id)
    {
        var dashboard = await GetByIdAsync(id);
        if (dashboard is not null && dashboard is UserDashboard userDashboard)
        {
            context.UserDashboards.Remove(userDashboard);
        }
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
