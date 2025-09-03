using LinaSys.Core.Domain.Aggregates.Dashboard;
using LinaSys.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Core.Infrastructure.Persistence.Repositories;

public class NotificationRepository(CoreDbContext context) : INotificationRepository
{
    public async Task<List<UserNotification>> GetUnreadByUserAsync(string userId)
    {
        return await context.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<List<UserNotification>> GetRecentByUserAsync(string userId, int count)
    {
        return await context.UserNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<UserNotification?> GetByIdAsync(long id)
    {
        return await context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task AddAsync(UserNotification notification)
    {
        await context.UserNotifications.AddAsync(notification);
    }

    public async Task UpdateAsync(UserNotification notification)
    {
        context.UserNotifications.Update(notification);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(long id)
    {
        var notification = await GetByIdAsync(id);
        if (notification is not null)
        {
            context.UserNotifications.Remove(notification);
        }
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
