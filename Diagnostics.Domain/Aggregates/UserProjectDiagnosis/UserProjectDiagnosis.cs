using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;

/// <summary>
/// Represents a user's complete diagnosis for a project across all phases.
/// This is the aggregate root that ensures consistency for all diagnosis operations.
/// </summary>
public class UserProjectDiagnosis : Entity, IAggregateRoot
{
    private readonly List<DiagnosisAnswer> _answers = [];
    private readonly List<DiagnosisPhaseSummary> _phaseSummaries = [];
    protected UserProjectDiagnosis()
    {
        // Required by EF Core
    }

    private UserProjectDiagnosis(long projectId, string userId, DateTime createdAt)
    {
        ProjectId = projectId;
        UserId = userId;
        CreatedAt = createdAt;
        Status = DiagnosisStatus.InProgress;
    }

    /// <summary>
    /// Gets the diagnosis answers.
    /// </summary>
    public IReadOnlyCollection<DiagnosisAnswer> Answers => _answers.AsReadOnly();

    /// <summary>
    /// Gets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last update date.
    /// </summary>
    public DateTime? LastUpdatedAt { get; private set; }

    /// <summary>
    /// Gets the phase summaries.
    /// </summary>
    public IReadOnlyCollection<DiagnosisPhaseSummary> PhaseSummaries => _phaseSummaries.AsReadOnly();

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// Gets the diagnosis status.
    /// </summary>
    public DiagnosisStatus Status { get; private set; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Creates a new user project diagnosis.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>A new UserProjectDiagnosis instance.</returns>
    public static UserProjectDiagnosis Create(long projectId, string userId, DateTime createdAt)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid project ID", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Invalid user ID", nameof(userId));
        }

        return new UserProjectDiagnosis(projectId, userId, createdAt);
    }

    /// <summary>
    /// Adds or updates answers from an approved form submission.
    /// </summary>
    /// <param name="phase">The question phase.</param>
    /// <param name="answerInputList">The list of answer inputs.</param>
    /// <param name="submittedAt">The submission date.</param>
    /// <param name="updatedAt">The update timestamp.</param>
    public void AddOrUpdateAnswersFromApprovedSubmission(
        Enums.QuestionPhase phase,
        IEnumerable<DiagnosisAnswerInput> answerInputList,
        DateTime submittedAt,
        DateTime updatedAt)
    {
        if (Status == DiagnosisStatus.Archived)
        {
            throw new InvalidOperationException("Cannot modify archived diagnosis");
        }

        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentException("Invalid phase", nameof(phase));
        }

        if (answerInputList is null)
        {
            throw new ArgumentNullException(nameof(answerInputList));
        }

        // Remove existing answers for this phase
        _answers.RemoveAll(a => a.Phase == phase);

        // Add new answers
        var order = 1;
        foreach (var answerInput in answerInputList)
        {
            var answer = DiagnosisAnswer.CreateFromSubmission(
                answerInput,
                phase,
                order++,
                submittedAt);

            _answers.Add(answer);
        }

        // Update phase summary
        UpdatePhaseSummary(phase, submittedAt);

        // Update timestamp
        LastUpdatedAt = updatedAt;

        // Check if diagnosis is complete
        CheckAndUpdateCompletionStatus();
    }

    /// <summary>
    /// Archives the diagnosis.
    /// </summary>
    /// <param name="archivedAt">The archive timestamp.</param>
    public void ArchiveDiagnosis(DateTime archivedAt)
    {
        if (Status == DiagnosisStatus.Archived)
        {
            return;
        }

        Status = DiagnosisStatus.Archived;
        LastUpdatedAt = archivedAt;
    }

    /// <summary>
    /// Gets answers by phase.
    /// </summary>
    /// <param name="phase">The question phase.</param>
    /// <returns>A collection of diagnosis answers for the specified phase.</returns>
    public IEnumerable<DiagnosisAnswer> GetAnswersByPhase(Enums.QuestionPhase phase)
    {
        return _answers.Where(a => a.Phase == phase).OrderBy(a => a.Order);
    }

    /// <summary>
    /// Gets the completion date for a phase.
    /// </summary>
    /// <param name="phase">The question phase.</param>
    /// <returns>The completion date if the phase is completed; otherwise, null.</returns>
    public DateTime? GetPhaseCompletionDate(Enums.QuestionPhase phase)
    {
        return _phaseSummaries.FirstOrDefault(s => s.Phase == phase)?.CompletedAt;
    }

    /// <summary>
    /// Checks if a phase has been completed.
    /// </summary>
    /// <param name="phase">The question phase.</param>
    /// <returns>True if the phase has been completed; otherwise, false.</returns>
    public bool HasPhase(Enums.QuestionPhase phase)
    {
        return _phaseSummaries.Any(s => s.Phase == phase);
    }

    /// <summary>
    /// Checks if a phase is completed.
    /// </summary>
    /// <param name="phase">The question phase.</param>
    /// <returns>True if the phase is completed; otherwise, false.</returns>
    public bool IsPhaseCompleted(Enums.QuestionPhase phase)
    {
        return _phaseSummaries.Any(p => p.Phase == phase);
    }

    /// <summary>
    /// Reactivates an archived diagnosis.
    /// </summary>
    /// <param name="reactivatedAt">The reactivation timestamp.</param>
    public void ReactivateDiagnosis(DateTime reactivatedAt)
    {
        if (Status != DiagnosisStatus.Archived)
        {
            return;
        }

        Status = DiagnosisStatus.InProgress;
        LastUpdatedAt = reactivatedAt;
    }

    private void CheckAndUpdateCompletionStatus()
    {
        // Business rule: Diagnosis is complete when both Start and Final phases have answers
        var hasStartPhase = _phaseSummaries.Any(s => s.Phase == Enums.QuestionPhase.Start);
        var hasFinalPhase = _phaseSummaries.Any(s => s.Phase == Enums.QuestionPhase.Final);

        if (hasStartPhase && hasFinalPhase && Status == DiagnosisStatus.InProgress)
        {
            Status = DiagnosisStatus.Completed;
        }
    }

    private void UpdatePhaseSummary(Enums.QuestionPhase phase, DateTime completedAt)
    {
        var summary = _phaseSummaries.FirstOrDefault(s => s.Phase == phase);
        if (summary is null)
        {
            summary = new DiagnosisPhaseSummary(phase, completedAt);
            _phaseSummaries.Add(summary);
        }
        else
        {
            summary.Update(completedAt);
        }

        // Calculate statistics
        var phaseAnswers = _answers.Where(a => a.Phase == phase).ToList();
        var answerCount = phaseAnswers.Count;
        summary.UpdateStatistics(answerCount);
    }
}
