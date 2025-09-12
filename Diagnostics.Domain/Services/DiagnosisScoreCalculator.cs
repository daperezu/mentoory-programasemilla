using LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;
using LinaSys.Diagnostics.Domain.ValueObjects;

namespace LinaSys.Diagnostics.Domain.Services;

/// <summary>
/// Domain service for calculating and aggregating diagnosis scores.
/// </summary>
public class DiagnosisScoreCalculator
{
    /// <summary>
    /// Calculates the aggregated score for a question from multiple answers.
    /// </summary>
    /// <param name="answers">The collection of diagnosis answers for a question.</param>
    /// <returns>The aggregated score.</returns>
    public static decimal CalculateQuestionScore(IEnumerable<DiagnosisAnswer> answers)
    {
        if (answers == null || !answers.Any())
        {
            return 0;
        }

        // For multi-select questions, sum the scores
        return answers.Sum(a => a.Score);
    }

    /// <summary>
    /// Determines the preferred source for a set of answers.
    /// </summary>
    /// <param name="answers">The collection of diagnosis answers.</param>
    /// <returns>The preferred source ("Coordinator" or "Starter").</returns>
    public string DeterminePreferredSource(IEnumerable<DiagnosisAnswer> answers)
    {
        if (!answers.Any())
        {
            return "Starter";
        }

        // If any coordinator answer has PreferredForDiagnosis = true, use coordinator
        var hasCoordinatorPreference = answers.Any(a =>
            a.AnswerSource == "Coordinator" &&
            a.PreferredForDiagnosis);

        return hasCoordinatorPreference ? "Coordinator" : "Starter";
    }

    /// <summary>
    /// Aggregates multi-select scores using summation.
    /// </summary>
    /// <param name="answers">The collection of diagnosis answers.</param>
    /// <returns>The aggregated score.</returns>
    public decimal AggregateMultiSelectScores(IEnumerable<DiagnosisAnswer> answers)
    {
        if (!answers.Any())
        {
            return 0;
        }

        // Default aggregation method is SUM
        return answers.Sum(a => a.Score);
    }

    /// <summary>
    /// Creates question scores from grouped answers.
    /// </summary>
    /// <param name="questionAnswers">Answers grouped by question.</param>
    /// <param name="blockId">The block identifier.</param>
    /// <returns>A collection of question scores.</returns>
    public IEnumerable<QuestionScore> CreateQuestionScores(
        IGrouping<long, DiagnosisAnswer> questionAnswers,
        long blockId)
    {
        var questionId = questionAnswers.Key;
        var answers = questionAnswers.ToList();

        if (!answers.Any())
        {
            yield break;
        }

        // Determine preferred source
        var preferredSource = DeterminePreferredSource(answers);
        // Filter answers by preferred source
        var preferredAnswers = answers
            .Where(a => a.AnswerSource == preferredSource)
            .ToList();

        if (!preferredAnswers.Any())
        {
            // Fallback to all answers if no preferred source found
            preferredAnswers = answers;
        }

        // Calculate aggregated score
        var score = AggregateMultiSelectScores(preferredAnswers);

        // Get internal question ID (assuming it's stored or we use order)
        var internalId = preferredAnswers.First().Order;

        // Create label in format "blockId.internalId"
        var label = $"{blockId}.{internalId}";

        yield return new QuestionScore(
            questionId,
            label,
            score,
            preferredSource,
            internalId);
    }

    /// <summary>
    /// Builds chart data for a block from its answers.
    /// </summary>
    /// <param name="blockId">The block identifier.</param>
    /// <param name="blockName">The block name.</param>
    /// <param name="blockAnswers">All answers for the block.</param>
    /// <param name="maxScore">The maximum possible score.</param>
    /// <returns>The block chart data.</returns>
    public BlockChartData BuildBlockChartData(
        long blockId,
        string blockName,
        IEnumerable<DiagnosisAnswer> blockAnswers,
        decimal maxScore = 10)
    {
        var answersList = blockAnswers.ToList();
        if (!answersList.Any())
        {
            // Return empty chart data with zero scores
            var emptyScore = new QuestionScore(0, $"{blockId}.0", 0, "Starter", 0);
            return new BlockChartData(blockId, blockName, [emptyScore], maxScore);
        }

        // Group answers by question
        var questionGroups = answersList.GroupBy(a => a.QuestionId);
        // Create question scores
        var scores = new List<QuestionScore>();
        foreach (var questionGroup in questionGroups)
        {
            scores.AddRange(CreateQuestionScores(questionGroup, blockId));
        }

        // Sort by internal question ID for consistent ordering
        scores = [.. scores.OrderBy(s => s.InternalQuestionId)];

        return new BlockChartData(blockId, blockName, scores, maxScore);
    }
}