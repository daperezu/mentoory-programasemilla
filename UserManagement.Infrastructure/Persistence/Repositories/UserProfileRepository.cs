using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using LinaSys.UserManagement.Domain.AggregatesModel.UserProfileAggregate;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.UserManagement.Infrastructure.Persistence.Repositories;

public class UserProfileRepository(UserManagementDbContext context) : AbstractRepository<UserProfile>(context), IUserProfileRepository
{
    private readonly UserManagementDbContext dbContext = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<UserProfile?> GetAsync(int userProfileId)
    {
        return await dbContext.UserProfiles
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == userProfileId);
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        return await dbContext.UserProfiles
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<UserProfile?> GetByIdentificationAsync(string identification)
    {
        return await dbContext.UserProfiles
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Identification == identification);
    }

    public async Task<IEnumerable<UserProfile>> GetActiveProfilesAsync()
    {
        return await dbContext.UserProfiles
            .Where(u => u.IsActive)
            .Include(u => u.Preferences)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProfile>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await dbContext.UserProfiles
            .Where(u => ids.Contains((int)u.Id))
            .Include(u => u.Preferences)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        return await dbContext.UserProfiles
            .AnyAsync(u => u.UserId == userId);
    }

    public async Task<bool> IdentificationExistsAsync(string identification, int? excludeUserProfileId = null)
    {
        var query = dbContext.UserProfiles
            .Where(u => u.Identification == identification);

        if (excludeUserProfileId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserProfileId.Value);
        }

        return await query.AnyAsync();
    }
}