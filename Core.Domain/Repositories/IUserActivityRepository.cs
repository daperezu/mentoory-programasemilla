using LinaSys.Core.Domain.Aggregates.Activity;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Repositories;

/// <summary>
/// Repository interface for user activities.
/// </summary>
public interface IUserActivityRepository : IRepository<UserActivity>
{
    /// <summary>
    /// Adds a new user activity.
    /// </summary>
    /// <param name="activity">The activity to add.</param>
    /// <returns>The added activity.</returns>
    UserActivity Add(UserActivity activity);

    /// <summary>
    /// Gets recent activities for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="count">The number of activities to retrieve.</param>
    /// <returns>A list of recent user activities.</returns>
    Task<List<UserActivity>> GetRecentActivitiesAsync(string userId, int count = 20);

    /// <summary>
    /// Gets activities by entity.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <returns>A list of activities for the specified entity.</returns>
    Task<List<UserActivity>> GetByEntityAsync(string entityType, long entityId);

    /// <summary>
    /// Gets activities within a date range.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>A list of activities within the date range.</returns>
    Task<List<UserActivity>> GetByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
}