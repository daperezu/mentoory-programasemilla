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

    /// <inheritdoc/>
    public async Task<IEnumerable<DiagnosisAnswer>> GetApprovedDiagnosisAnswersAsync(
        long projectId,
        string userId,
        Domain.Enums.QuestionPhase phase,
        CancellationToken cancellationToken = default)
    {
        var diagnosis = await context.Set<UserProjectDiagnosis>()
            .Include(d => d.Answers)
            .FirstOrDefaultAsync(
                d => d.ProjectId == projectId && d.UserId == userId,
                cancellationToken);

        if (diagnosis is null)
        {
            return [];
        }

        // Filter answers by phase and return only those used for diagnosis
        return diagnosis.Answers
            .Where(a => a.Phase == phase && a.IsUsedForDiagnosis)
            .OrderBy(a => a.BlockId)
            .ThenBy(a => a.Order)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<(long BlockId, string BlockName, int QuestionCount)>> GetBlocksWithQuestionsAsync(
        long projectId,
        CancellationToken cancellationToken = default)
    {
        // Get all diagnoses for the project to get block information
        var diagnoses = await context.Set<UserProjectDiagnosis>()
            .Include(d => d.Answers)
            .Where(d => d.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        if (!diagnoses.Any())
        {
            return [];
        }

        // Extract unique blocks with their question counts
        var blocks = diagnoses
            .SelectMany(d => d.Answers)
            .Where(a => a.IsUsedForDiagnosis)
            .GroupBy(a => new { a.BlockId, a.BlockName })
            .Select(g => (
                BlockId: g.Key.BlockId,
                BlockName: g.Key.BlockName ?? $"Bloque {g.Key.BlockId}",
                QuestionCount: g.Select(a => a.QuestionId).Distinct().Count()))
            .OrderBy(b => b.BlockId)
            .ToList();

        return blocks;
    }
}
