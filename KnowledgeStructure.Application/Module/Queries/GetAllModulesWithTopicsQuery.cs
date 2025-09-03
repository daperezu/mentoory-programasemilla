using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetAllModulesWithTopicsQuery : IBaseRequest<List<ModuleWithTopicsDto>>;

public sealed record ModuleWithTopicsDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<TopicDto> Topics { get; set; } = [];
}

public sealed record TopicDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class GetAllBlocksQueryHandler(IModuleRepository repository)
    : BaseCommandHandler<GetAllModulesWithTopicsQuery, List<ModuleWithTopicsDto>>
{
    public override async Task<Result<List<ModuleWithTopicsDto>>> Handle(GetAllModulesWithTopicsQuery request, CancellationToken cancellationToken)
    {
        var modules = await repository.GetAllModulesWithTopicsAsync(cancellationToken).ConfigureAwait(false);

        var result = modules.Select(m => new ModuleWithTopicsDto
        {
            Id = m.Id,
            Name = m.Name,
            /*Topics = m.Topics.Select(t => new TopicDto
            {
                Id = t.Id,
                Name = t.Name,
            }).ToList(),*/
        }).ToList();

        return Success(result);
    }
}
