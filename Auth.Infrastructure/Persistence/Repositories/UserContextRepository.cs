using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Auth.Infrastructure.Persistence.Repositories;

public class UserContextRepository(AuthDbContext dbContext, UserManager<User> userManager) : IUserContextRepository
{

    /// <inheritdoc/>
    public async Task<UserContextPreferences?> GetUserContextPreferencesAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserContextPreferences
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveUserContextPreferencesAsync(UserContextPreferences preferences, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserContextPreferences
            .FirstOrDefaultAsync(p => p.UserId == preferences.UserId, cancellationToken);

        if (existing != null)
        {
            existing.LastRole = preferences.LastRole;
            existing.LastIncubatorId = preferences.LastIncubatorId;
            existing.LastProjectId = preferences.LastProjectId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            preferences.UpdatedAt = DateTime.UtcNow;
            await dbContext.UserContextPreferences.AddAsync(preferences, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UserHasRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false; // User does not exist
        }

        var roles = await userManager.GetRolesAsync(user);

        return roles.Contains(role);
    }
}
