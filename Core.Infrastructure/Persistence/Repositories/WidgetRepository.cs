using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Core.Infrastructure.Persistence.Repositories;

public class WidgetRepository(CoreDbContext context) : IWidgetRepository
{
    public async Task<List<DashboardWidget>> GetByRoleAsync(string role)
    {
        return await context.DashboardWidgets
            .Where(w => w.Roles != null && w.Roles.Contains(role) && w.IsActive)
            .OrderBy(w => w.DefaultPosition)
            .ToListAsync();
    }

    public async Task<List<UserWidgetConfiguration>> GetUserConfigurationsAsync(string userId)
    {
        return await context.UserWidgetConfigurations
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task SaveUserConfigurationAsync(UserWidgetConfiguration configuration)
    {
        var existing = await context.UserWidgetConfigurations
            .FirstOrDefaultAsync(c => c.UserId == configuration.UserId &&
                                     c.WidgetId == configuration.WidgetId);

        if (existing is not null)
        {
            existing.UpdatePosition(configuration.Position);
            existing.UpdateSize(configuration.Width, configuration.Height);
            existing.UpdateVisibility(configuration.IsVisible);
            existing.UpdateConfiguration(configuration.Configuration);
        }
        else
        {
            await context.UserWidgetConfigurations.AddAsync(configuration);
        }

        await context.SaveChangesAsync();
    }

    public async Task<DashboardWidget?> GetByCodeAsync(string code)
    {
        return await context.DashboardWidgets
            .FirstOrDefaultAsync(w => w.Name == code);
    }

    public async Task<List<DashboardWidget>> GetAllAsync()
    {
        return await context.DashboardWidgets
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }
}
