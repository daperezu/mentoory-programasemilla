using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetStructureModuleWithTopicsQuery(long ModuleId) : IBaseRequest<StructureModuleWithTopicsDto>;

public sealed record StructureModuleWithTopicsDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<StructureModuleTopicDto> Topics { get; set; } = [];
}

public sealed record StructureModuleTopicDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }
}

public sealed class GetStructureModuleWithTopicsQueryHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<GetStructureModuleWithTopicsQuery, StructureModuleWithTopicsDto>
{
    public override async Task<Result<StructureModuleWithTopicsDto>> Handle(
        GetStructureModuleWithTopicsQuery request,
        CancellationToken cancellationToken)
    {
        var module = await repository.GetModuleWithTopicsAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                ("Module", $"Módulo con ID {request.ModuleId} no encontrado"));
        }

        var dto = new StructureModuleWithTopicsDto
        {
            Id = module.Id,
            Name = module.Module.Name,
            Topics = module.KnowledgeStructureTopics
                .OrderBy(t => t.Order)
                .Select(t => new StructureModuleTopicDto
                {
                    Id = t.Id,
                    Name = t.Topic.Name,
                    Order = t.Order,
                })
                .ToList(),
        };

        return Success(dto);
    }
}
