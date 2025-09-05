using LinaSys.Core.Domain.Aggregates.Activity;
using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Core.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for user activities.
/// </summary>
public class UserActivityRepository(CoreDbContext context) : AbstractRepository<UserActivity>(context), IUserActivityRepository
{
    public async Task<List<UserActivity>> GetRecentActivitiesAsync(string userId, int count = 20)
    {
        return await context.UserActivities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<UserActivity>> GetByEntityAsync(string entityType, long entityId)
    {
        return await context.UserActivities
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<UserActivity>> GetByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await context.UserActivities
            .Where(a => a.UserId == userId && a.CreatedDate >= startDate && a.CreatedDate <= endDate)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }
}
