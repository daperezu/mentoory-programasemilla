using LinaSys.Shared.Domain;
using System.Collections.Generic;
using System.Linq;

namespace LinaSys.UserProjectDiagnosis.Domain.ValueObjects;

/// <summary>
/// Data for one radial chart representing a block.
/// </summary>
public class BlockChartData : ValueObject
{
    private readonly List<QuestionScore> _scores;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockChartData"/> class.
    /// </summary>
    /// <param name="blockId">The block identifier.</param>
    /// <param name="blockName">The block name.</param>
    /// <param name="scores">The question scores for this block.</param>
    /// <param name="maxScore">The maximum score for normalization.</param>
    public BlockChartData(long blockId, string blockName, IEnumerable<QuestionScore> scores, decimal maxScore)
    {
        BlockId = blockId;
        BlockName = blockName;
        _scores = scores?.ToList() ?? new List<QuestionScore>();
        MaxScore = maxScore;
    }

    /// <summary>
    /// Gets the block identifier.
    /// </summary>
    public long BlockId { get; }

    /// <summary>
    /// Gets the block name.
    /// </summary>
    public string BlockName { get; }

    /// <summary>
    /// Gets the question scores for this block.
    /// </summary>
    public IReadOnlyList<QuestionScore> Scores => _scores.AsReadOnly();

    /// <summary>
    /// Gets the maximum score for normalization.
    /// </summary>
    public decimal MaxScore { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return BlockId;
        yield return BlockName;
        yield return MaxScore;
        foreach (var score in _scores)
        {
            yield return score;
        }
    }
}