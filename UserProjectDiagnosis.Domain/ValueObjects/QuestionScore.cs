using LinaSys.Shared.Domain;
using System.Collections.Generic;

namespace LinaSys.UserProjectDiagnosis.Domain.ValueObjects;

/// <summary>
/// Encapsulates final score for a question.
/// </summary>
public class QuestionScore : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuestionScore"/> class.
    /// </summary>
    /// <param name="questionId">The question identifier.</param>
    /// <param name="label">The label in format "blockId.internalId".</param>
    /// <param name="score">The calculated score.</param>
    /// <param name="source">The source of the score ("Starter" or "Coordinator").</param>
    public QuestionScore(long questionId, string label, decimal score, string source)
    {
        QuestionId = questionId;
        Label = label;
        Score = score;
        Source = source;
    }

    /// <summary>
    /// Gets the question identifier.
    /// </summary>
    public long QuestionId { get; }

    /// <summary>
    /// Gets the label in format "blockId.internalId".
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the calculated score.
    /// </summary>
    public decimal Score { get; }

    /// <summary>
    /// Gets the source of the score.
    /// </summary>
    public string Source { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return QuestionId;
        yield return Label;
        yield return Score;
        yield return Source;
    }
}