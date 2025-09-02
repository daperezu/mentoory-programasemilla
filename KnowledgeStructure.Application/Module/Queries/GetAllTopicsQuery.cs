using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetAllTopicsQuery : IBaseRequest<Dictionary<long, string>>;

public sealed class GetAllTopicsQueryHandler(IModuleRepository repository)
    : BaseCommandHandler<GetAllTopicsQuery, Dictionary<long, string>>
{
    public override async Task<Result<Dictionary<long, string>>> Handle(GetAllTopicsQuery request, CancellationToken cancellationToken)
    {
        var modules = await repository.GetAllTopicsAsync(cancellationToken).ConfigureAwait(false);

        var result = modules.ToDictionary(b => b.Id, b => b.Name);
        return Success(result);
    }
}
