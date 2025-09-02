using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;

public sealed record GetKnowledgeStructureWithModulesQuery(long KnowledgeStructureId) : IBaseRequest<KnowledgeStructureWithModulesDto>;

public sealed record KnowledgeStructureWithModulesDto(
    long Id,
    string Name,
    string? Description,
    List<ModuleSimpleDto> Modules);

public sealed record ModuleSimpleDto(
    long Id,
    string Name,
    int Order);

public sealed class GetKnowledgeStructureWithModulesQueryHandler(
    IKnowledgeStructureRepository repository) : BaseCommandHandler<GetKnowledgeStructureWithModulesQuery, KnowledgeStructureWithModulesDto>
{
    public override async Task<Result<KnowledgeStructureWithModulesDto>> Handle(
        GetKnowledgeStructureWithModulesQuery request,
        CancellationToken cancellationToken)
    {
        var knowledgeStructure = await repository.GetWithModulesAndTopicsByIdAsync(
            request.KnowledgeStructureId,
            cancellationToken);

        if (knowledgeStructure is null)
        {
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                ("KnowledgeStructureId", "Estructura de conocimiento no encontrada"));
        }

        var dto = new KnowledgeStructureWithModulesDto(
            knowledgeStructure.Id,
            knowledgeStructure.Name,
            knowledgeStructure.Description,
            knowledgeStructure.KnowledgeStructureModules
                .OrderBy(m => m.Order)
                .Select(m => new ModuleSimpleDto(
                    m.Module.Id,
                    m.Module.Name,
                    m.Order))
                .ToList());

        return Success(dto);
    }
}