namespace LinaSys.Auth.Domain.AggregatesModel.User;

/// <summary>
/// Entity for storing user context preferences.
/// </summary>
public class UserContextPreferences
{
    /// <summary>
    /// Gets or sets the user identifier (Primary Key).
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the last selected role identifier.
    /// </summary>
    public string? LastRole { get; set; }

    /// <summary>
    /// Gets or sets the last selected incubator identifier.
    /// </summary>
    public long? LastIncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the last selected project identifier.
    /// </summary>
    public long? LastProjectId { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public virtual User User { get; set; } = default!;
}
