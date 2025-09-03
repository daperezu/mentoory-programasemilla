using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Block.Queries;

public sealed record GetAllBlocksQuery : IBaseRequest<Dictionary<long, string>>;

public sealed class GetAllBlocksQueryHandler(IBlockRepository repository)
    : BaseCommandHandler<GetAllBlocksQuery, Dictionary<long, string>>
{
    public override async Task<Result<Dictionary<long, string>>> Handle(GetAllBlocksQuery request, CancellationToken cancellationToken)
    {
        var blocks = await repository.GetAllBlocksAsync(cancellationToken).ConfigureAwait(false);

        var result = blocks.ToDictionary(b => b.Id, b => b.Name);
        return Success(result);
    }
}
