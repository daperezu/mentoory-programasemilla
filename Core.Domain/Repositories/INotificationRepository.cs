using LinaSys.Core.Domain.Aggregates.Dashboard;

namespace LinaSys.Core.Domain.Repositories;

public interface INotificationRepository
{
    Task<List<UserNotification>> GetUnreadByUserAsync(string userId);
    Task<List<UserNotification>> GetRecentByUserAsync(string userId, int count);
    Task<UserNotification?> GetByIdAsync(long id);
    Task AddAsync(UserNotification notification);
    Task UpdateAsync(UserNotification notification);
    Task DeleteAsync(long id);
    Task SaveChangesAsync();
}