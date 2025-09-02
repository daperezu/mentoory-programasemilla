using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;

/// <summary>
/// Value object representing the completion status of a diagnosis phase.
/// </summary>
public class DiagnosisPhaseSummary : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosisPhaseSummary"/> class.
    /// </summary>
    /// <param name="phase">The question phase.</param>
    /// <param name="completedAt">The completion date.</param>
    public DiagnosisPhaseSummary(QuestionPhase phase, DateTime completedAt)
    {
        Phase = phase;
        CompletedAt = completedAt;
        AnswerCount = 0;
    }

    protected DiagnosisPhaseSummary()
    {
        // Required by EF Core
    }

    /// <summary>
    /// Gets the number of answers in this phase.
    /// </summary>
    public int AnswerCount { get; private set; }

    /// <summary>
    /// Gets the completion date.
    /// </summary>
    public DateTime CompletedAt { get; private set; }

    /// <summary>
    /// Gets the question phase.
    /// </summary>
    public QuestionPhase Phase { get; private set; }

    /// <summary>
    /// Updates the completion date.
    /// </summary>
    /// <param name="completedAt">The new completion date.</param>
    internal void Update(DateTime completedAt)
    {
        CompletedAt = completedAt;
    }

    /// <summary>
    /// Updates the statistics for this phase.
    /// </summary>
    /// <param name="answerCount">The number of answers.</param>
    internal void UpdateStatistics(int answerCount)
    {
        AnswerCount = answerCount;
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Phase;
    }
}
