using LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Diagnostics.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing user project diagnoses.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserProjectDiagnosisRepository"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class UserProjectDiagnosisRepository(DiagnosticsDbContext context) : AbstractRepository<UserProjectDiagnosis>(context), IUserProjectDiagnosisRepository
{
    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(
        long projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<UserProjectDiagnosis>()
            .AnyAsync(d => d.ProjectId == projectId && d.UserId == userId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<UserProjectDiagnosis?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var diagnosis = await context.Set<UserProjectDiagnosis>()
            .Include(d => d.Answers)
            .Include(d => d.PhaseSummaries)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return diagnosis;
    }

    /// <inheritdoc/>
    public async Task<UserProjectDiagnosis?> GetByProjectAndUserAsync(
        long projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<UserProjectDiagnosis>()
            .Include(d => d.Answers)
            .Include(d => d.PhaseSummaries)
            .FirstOrDefaultAsync(
                d => d.ProjectId == projectId && d.UserId == userId,
                cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<UserProjectDiagnosis>> GetByProjectAsync(
        long projectId,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<UserProjectDiagnosis>()
            .Include(d => d.Answers)
            .Include(d => d.PhaseSummaries)
            .Where(d => d.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<UserProjectDiagnosis>> GetByStatusAsync(
        DiagnosisStatus status,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<UserProjectDiagnosis>()
            .Include(d => d.Answers)
            .Include(d => d.PhaseSummaries)
            .Where(d => d.Status == status)
            .ToListAsync(cancellationToken);
    }
}
