using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetStructureModulesByKnowledgeStructureQuery(long KnowledgeStructureId) : IBaseRequest<List<StructureModuleDto>>;

public sealed record StructureModuleDto(long StructureModuleId, string ModuleName, int Order);

public sealed class GetStructureModulesByKnowledgeStructureQueryHandler(
    IKnowledgeStructureRepository repository) : BaseCommandHandler<GetStructureModulesByKnowledgeStructureQuery, List<StructureModuleDto>>
{
    public override async Task<Result<List<StructureModuleDto>>> Handle(GetStructureModulesByKnowledgeStructureQuery request, CancellationToken cancellationToken)
    {
        var allModules = await repository.GetAllStructureModulesAsync(cancellationToken);

        var modules = allModules
            .Where(sm => sm.KnowledgeStructureId == request.KnowledgeStructureId)
            .OrderBy(sm => sm.Order)
            .Select(sm => new StructureModuleDto(
                sm.Id,
                sm.Module.Name,
                sm.Order))
            .ToList();

        return Success(modules);
    }
}
