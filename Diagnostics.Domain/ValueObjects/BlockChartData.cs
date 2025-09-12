using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.ValueObjects;

/// <summary>
/// Represents data for one radial chart displaying a block's scores.
/// </summary>
public class BlockChartData : ValueObject
{
    private readonly List<QuestionScore> _scores;

    public BlockChartData(long blockId, string blockName, IEnumerable<QuestionScore> scores, decimal maxScore)
    {
        if (blockId <= 0)
        {
            throw new ArgumentException("Block ID must be positive", nameof(blockId));
        }

        if (string.IsNullOrWhiteSpace(blockName))
        {
            throw new ArgumentException("Block name cannot be empty", nameof(blockName));
        }

        if (scores == null || !scores.Any())
        {
            throw new ArgumentException("Scores cannot be null or empty", nameof(scores));
        }

        if (maxScore <= 0)
        {
            throw new ArgumentException("Max score must be positive", nameof(maxScore));
        }

        BlockId = blockId;
        BlockName = blockName;
        _scores = scores.ToList();
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
    /// Gets the maximum possible score for normalization.
    /// </summary>
    public decimal MaxScore { get; }

    protected override IEnumerable<object> GetEqualityComponents()
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