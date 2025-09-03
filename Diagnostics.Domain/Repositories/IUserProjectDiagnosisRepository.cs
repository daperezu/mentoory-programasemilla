using LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Repositories;

/// <summary>
/// Repository for managing user project diagnoses following DDD principles.
/// </summary>
public interface IUserProjectDiagnosisRepository : IRepository<UserProjectDiagnosis>
{
    /// <summary>
    /// Adds a new user project diagnosis to the repository.
    /// </summary>
    /// <param name="entity">The user project diagnosis to add.</param>
    /// <returns>The added user project diagnosis.</returns>
    UserProjectDiagnosis Add(UserProjectDiagnosis entity);

    /// <summary>
    /// Checks if a diagnosis exists for a user and project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a diagnosis exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(
        long projectId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user project diagnosis by its identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user project diagnosis if found; otherwise, null.</returns>
    Task<UserProjectDiagnosis?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's diagnosis for a specific project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user project diagnosis if found; otherwise, null.</returns>
    Task<UserProjectDiagnosis?> GetByProjectAndUserAsync(
        long projectId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all diagnoses for a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of user project diagnoses.</returns>
    Task<List<UserProjectDiagnosis>> GetByProjectAsync(
        long projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets diagnoses by status.
    /// </summary>
    /// <param name="status">The diagnosis status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of user project diagnoses with the specified status.</returns>
    Task<List<UserProjectDiagnosis>> GetByStatusAsync(
        DiagnosisStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user project diagnosis.
    /// </summary>
    /// <param name="entity">The user project diagnosis to update.</param>
    void Update(UserProjectDiagnosis entity);
}
