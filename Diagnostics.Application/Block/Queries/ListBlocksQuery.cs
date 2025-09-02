using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Block.Queries;

public sealed record ListBlocksQuery(
    int Start,
    int Length,
    string? Name,
    string OrderByColumn,
    string OrderDirection) : IBaseRequest<FilteredQueryResult<BlockListDto>>;

public sealed record BlockListDto(
    long Id,
    string Name,
    int QuestionCount);

public sealed class ListBlocksQueryHandler(IBlockRepository repository)
    : BaseCommandHandler<ListBlocksQuery, FilteredQueryResult<BlockListDto>>
{
    public override async Task<Result<FilteredQueryResult<BlockListDto>>> Handle(
        ListBlocksQuery request,
        CancellationToken cancellationToken)
    {
        var blocksWithCounts = await repository.GetAllBlocksWithQuestionCountAsync(cancellationToken);

        // Apply name filter
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            blocksWithCounts = blocksWithCounts
                .Where(b => b.Block.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var totalRecords = blocksWithCounts.Count;

        // Apply sorting
        var sortedBlocks = request.OrderByColumn.ToLowerInvariant() switch
        {
            "name" when request.OrderDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                => blocksWithCounts.OrderByDescending(b => b.Block.Name),
            "name" => blocksWithCounts.OrderBy(b => b.Block.Name),
            "questioncount" when request.OrderDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                => blocksWithCounts.OrderByDescending(b => b.QuestionCount),
            "questioncount" => blocksWithCounts.OrderBy(b => b.QuestionCount),
            _ => blocksWithCounts.OrderBy(b => b.Block.Name),
        };

        // Apply pagination
        var pagedBlocks = sortedBlocks
            .Skip(request.Start)
            .Take(request.Length)
            .ToList();

        var dtos = pagedBlocks.Select(b => new BlockListDto(
            b.Block.Id,
            b.Block.Name,
            b.QuestionCount)).ToList();

        return Success(FilteredQueryResult.From(dtos, totalRecords, totalRecords));
    }
}
