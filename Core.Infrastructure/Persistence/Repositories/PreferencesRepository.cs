using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LinaSys.Core.Infrastructure.Persistence.Repositories;

public class PreferencesRepository(CoreDbContext context, IMemoryCache cache) : AbstractRepository<UserDashboard>(context), IPreferencesRepository
{
    private const string CacheKeyPrefix = "user_preferences_";
    private readonly CoreDbContext dbContext = context;
    private readonly IMemoryCache _cache = cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public async Task<DashboardPreferences?> GetUserPreferencesAsync(string userId)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";

        if (_cache.TryGetValue(cacheKey, out DashboardPreferences? cached))
        {
            return cached;
        }

        var dashboard = await dbContext.UserDashboards
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (dashboard is null)
        {
            return null;
        }

        var preferences = dashboard.Preferences;

        if (preferences is not null)
        {
            _cache.Set(cacheKey, preferences, _cacheExpiration);
        }

        return preferences;
    }

    public async Task SaveUserPreferencesAsync(string userId, DashboardPreferences preferences)
    {
        var dashboard = await dbContext.UserDashboards
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (dashboard is not null)
        {
            dashboard.UpdatePreferences(preferences);
            // SaveChangesAsync should be called via UnitOfWork

            // Update cache
            var cacheKey = $"{CacheKeyPrefix}{userId}";
            _cache.Set(cacheKey, preferences, _cacheExpiration);
        }
    }

    public async Task DeleteUserPreferencesAsync(string userId)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";
        _cache.Remove(cacheKey);

        var dashboard = await dbContext.UserDashboards
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (dashboard is not null)
        {
            dashboard.UpdatePreferences(new DashboardPreferences());
            // SaveChangesAsync should be called via UnitOfWork
        }
    }
}
