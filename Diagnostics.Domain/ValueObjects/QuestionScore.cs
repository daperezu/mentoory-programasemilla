using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.ValueObjects;

/// <summary>
/// Encapsulates the final score for a question in a diagnosis chart.
/// </summary>
public class QuestionScore : ValueObject
{
    public QuestionScore(long questionId, string label, decimal score, string source, int internalQuestionId = 0)
    {
        if (questionId <= 0)
        {
            throw new ArgumentException("Question ID must be positive", nameof(questionId));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Label cannot be empty", nameof(label));
        }

        if (score < 0)
        {
            throw new ArgumentException("Score cannot be negative", nameof(score));
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source cannot be empty", nameof(source));
        }

        QuestionId = questionId;
        Label = label;
        Score = score;
        Source = source;
        InternalQuestionId = internalQuestionId;
    }

    /// <summary>
    /// Gets the question identifier.
    /// </summary>
    public long QuestionId { get; }

    /// <summary>
    /// Gets the label for the chart (format: "blockId.internalId").
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the aggregated score for this question.
    /// </summary>
    public decimal Score { get; }

    /// <summary>
    /// Gets the source of the score ("Starter" or "Coordinator").
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets the internal question ID within the block.
    /// </summary>
    public int InternalQuestionId { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return QuestionId;
        yield return Label;
        yield return Score;
        yield return Source;
    }
}