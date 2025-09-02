namespace LinaSys.Auth.Application.Interfaces;

/// <summary>
/// Service interface for project access operations.
/// </summary>
public interface IProjectAccessService
{
    /// <summary>
    /// Gets user's accessible projects within an incubator.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role identifier.</param>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of project information.</returns>
    Task<List<ProjectInfo>> GetUserProjectsAsync(string userId, string role, long incubatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a user has access to a project.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    Task<bool> ValidateProjectAccessAsync(string userId, long projectId, long incubatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a project exists in an incubator.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> ProjectExistsInIncubatorAsync(long projectId, long incubatorId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Project information DTO.
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string? UserRole { get; set; }
}
