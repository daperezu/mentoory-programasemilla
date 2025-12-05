namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents a user assigned to a project with a specific role.
/// </summary>
public class ProjectUser
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string Role { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the user is currently active in the project.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when the user joined the project.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets when the user left the project.
    /// </summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// Gets or sets who invited this user to the project.
    /// </summary>
    public string? InvitedBy { get; set; }

    /// <summary>
    /// Gets or sets additional metadata in JSON format.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who last updated the record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets navigation property to the project.
    /// </summary>
    public virtual Project Project { get; set; } = default!;
}