namespace LinaSys.Core.Application.Dashboard.Services;

public interface IActivityTrackingService
{
    Task TrackActivityAsync(
        string userId,
        string activityType,
        string description,
        string? entityType = null,
        long? entityId = null,
        string? metadata = null);

    Task<List<UserActivityDto>> GetRecentActivitiesAsync(string userId, int count = 20);
    Task<List<UserActivityDto>> GetActivitiesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<Dictionary<string, int>> GetActivitySummaryAsync(string userId, int days = 30);
    Task<DateTime?> GetLastActivityDateAsync(string userId);
}

public class UserActivityDto
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedDate { get; set; }
}