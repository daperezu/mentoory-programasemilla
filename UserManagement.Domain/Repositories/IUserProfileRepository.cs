using LinaSys.Shared.Domain.SeedWork;
using LinaSys.UserManagement.Domain.AggregatesModel.UserProfileAggregate;

namespace LinaSys.UserManagement.Domain.Repositories;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    UserProfile Add(UserProfile userProfile);
    void Update(UserProfile userProfile);
    Task<UserProfile?> GetAsync(int userProfileId);
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<UserProfile?> GetByIdentificationAsync(string identification);
    Task<IEnumerable<UserProfile>> GetActiveProfilesAsync();
    Task<IEnumerable<UserProfile>> GetByIdsAsync(IEnumerable<int> ids);
    Task<bool> ExistsAsync(string userId);
    Task<bool> IdentificationExistsAsync(string identification, int? excludeUserProfileId = null);
}