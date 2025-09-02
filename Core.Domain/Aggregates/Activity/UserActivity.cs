using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Activity;

/// <summary>
/// Represents a user activity in the system.
/// </summary>
public class UserActivity : Entity, IAggregateRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserActivity"/> class.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="activityType">The type of activity.</param>
    /// <param name="description">The activity description.</param>
    /// <param name="entityType">The entity type related to this activity.</param>
    /// <param name="entityId">The entity ID related to this activity.</param>
    /// <param name="metadata">Additional metadata as JSON.</param>
    public UserActivity(
        string userId,
        string activityType,
        string description,
        string? entityType = null,
        long? entityId = null,
        string? metadata = null)
        : this()
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        ActivityType = activityType ?? throw new ArgumentNullException(nameof(activityType));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        EntityType = entityType;
        EntityId = entityId;
        Metadata = metadata;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserActivity"/> class.
    /// </summary>
    protected UserActivity()
    {
        UserId = string.Empty;
        ActivityType = string.Empty;
        Description = string.Empty;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the activity type.
    /// </summary>
    public string ActivityType { get; private set; }

    /// <summary>
    /// Gets the activity description.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the entity type related to this activity.
    /// </summary>
    public string? EntityType { get; private set; }

    /// <summary>
    /// Gets the entity ID related to this activity.
    /// </summary>
    public long? EntityId { get; private set; }

    /// <summary>
    /// Gets the metadata as JSON string.
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Gets the creation date of the activity.
    /// </summary>
    public DateTime CreatedDate { get; private set; }

    /// <summary>
    /// Gets the user name (for display purposes).
    /// </summary>
    public string? UserName { get; private set; }

    /// <summary>
    /// Updates the metadata for this activity.
    /// </summary>
    /// <param name="metadata">The new metadata.</param>
    public void UpdateMetadata(string? metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Sets the user name for display purposes.
    /// </summary>
    /// <param name="userName">The user name.</param>
    public void SetUserName(string? userName)
    {
        UserName = userName;
    }
}