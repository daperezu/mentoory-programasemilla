using LinaSys.UserProjectDiagnosis.Domain.Aggregates.DiagnosisAnswer;
using LinaSys.UserProjectDiagnosis.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace LinaSys.UserProjectDiagnosis.Domain.Services;

/// <summary>
/// Domain service for score aggregation logic.
/// </summary>
public class DiagnosisScoreCalculator
{
    /// <summary>
    /// Calculates the score for a question based on provided answers.
    /// </summary>
    /// <param name="answers">The diagnosis answers for a question.</param>
    /// <returns>The calculated score.</returns>
    public decimal CalculateQuestionScore(IEnumerable<DiagnosisAnswer> answers)
    {
        if (answers == null || !answers.Any())
        {
            return 0;
        }

        // Sum scores for multi-select questions
        return answers.Sum(a => a.Score ?? 0);
    }

    /// <summary>
    /// Determines the preferred source for diagnosis answers.
    /// </summary>
    /// <param name="answers">All answers for a question.</param>
    /// <returns>The preferred answers based on PreferredForDiagnosis flag.</returns>
    public IEnumerable<DiagnosisAnswer> DeterminePreferredSource(IEnumerable<DiagnosisAnswer> answers)
    {
        if (answers == null || !answers.Any())
        {
            return Enumerable.Empty<DiagnosisAnswer>();
        }

        var groupedAnswers = answers.GroupBy(a => a.QuestionId);
        var preferredAnswers = new List<DiagnosisAnswer>();

        foreach (var group in groupedAnswers)
        {
            // Check if any coordinator answer has PreferredForDiagnosis = true
            var coordinatorPreferred = group.Any(a => 
                a.AnswerSource == AnswerSource.Coordinator && 
                a.PreferredForDiagnosis);

            if (coordinatorPreferred)
            {
                // Use only coordinator answers
                preferredAnswers.AddRange(group.Where(a => a.AnswerSource == AnswerSource.Coordinator));
            }
            else
            {
                // Use starter answers by default
                preferredAnswers.AddRange(group.Where(a => a.AnswerSource == AnswerSource.Starter));
            }
        }

        return preferredAnswers;
    }

    /// <summary>
    /// Aggregates scores for multi-select questions.
    /// </summary>
    /// <param name="answers">The answers to aggregate.</param>
    /// <returns>The aggregated score.</returns>
    public decimal AggregateMultiSelectScores(IEnumerable<DiagnosisAnswer> answers)
    {
        if (answers == null || !answers.Any())
        {
            return 0;
        }

        // Default aggregation is sum
        return answers.Sum(a => a.Score ?? 0);
    }

    /// <summary>
    /// Creates question scores from diagnosis answers.
    /// </summary>
    /// <param name="answers">The diagnosis answers grouped by question.</param>
    /// <param name="blockId">The block identifier.</param>
    /// <returns>Collection of question scores.</returns>
    public IEnumerable<QuestionScore> CreateQuestionScores(
        IEnumerable<IGrouping<long, DiagnosisAnswer>> answerGroups,
        long blockId)
    {
        var scores = new List<QuestionScore>();

        foreach (var group in answerGroups)
        {
            var preferredAnswers = DeterminePreferredSource(group);
            var score = CalculateQuestionScore(preferredAnswers);
            var source = preferredAnswers.Any(a => a.AnswerSource == AnswerSource.Coordinator)
                ? "Coordinator"
                : "Starter";

            // Get internal question ID from first answer
            var internalId = group.FirstOrDefault()?.InternalQuestionId ?? 0;
            var label = $"{blockId}.{internalId}";

            scores.Add(new QuestionScore(group.Key, label, score, source));
        }

        return scores;
    }
}