using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetModuleKnowledgeStructuresQuery(long ModuleId) : IBaseRequest<ModuleKnowledgeStructuresDto>;

public sealed record ModuleKnowledgeStructuresDto
{
    public long ModuleId { get; init; }

    public string ModuleName { get; init; } = string.Empty;

    public List<KnowledgeStructureAssignmentDto> KnowledgeStructures { get; init; } = [];
}

public sealed record KnowledgeStructureAssignmentDto
{
    public long Id { get; init; }

    public long StructureModuleId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int Order { get; init; }

    public bool IsActive { get; init; }

    public int TopicCount { get; init; }
}

public sealed class GetModuleKnowledgeStructuresQueryValidator : AbstractValidator<GetModuleKnowledgeStructuresQuery>
{
    public GetModuleKnowledgeStructuresQueryValidator()
    {
        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("El ID del módulo debe ser mayor a 0.");
    }
}

public sealed class GetModuleKnowledgeStructuresQueryHandler(
    IModuleRepository moduleRepository,
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<GetModuleKnowledgeStructuresQuery, ModuleKnowledgeStructuresDto>
{
    public override async Task<Result<ModuleKnowledgeStructuresDto>> Handle(
        GetModuleKnowledgeStructuresQuery request,
        CancellationToken cancellationToken)
    {
        // Get the module
        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.ModuleId), "El módulo no existe."));
        }

        // Get all structure modules for this module
        var allStructureModules = await knowledgeStructureRepository.GetAllStructureModulesAsync(cancellationToken);
        var moduleStructures = allStructureModules
            .Where(sm => sm.ModuleId == request.ModuleId)
            .ToList();

        var knowledgeStructures = moduleStructures
            .Select(sm => new KnowledgeStructureAssignmentDto
            {
                Id = sm.KnowledgeStructureId,
                StructureModuleId = sm.Id,
                Name = sm.KnowledgeStructure.Name,
                Description = sm.KnowledgeStructure.Description,
                Order = sm.Order,
                IsActive = sm.KnowledgeStructure.IsActive,
                TopicCount = sm.KnowledgeStructureTopics.Count,
            })
            .OrderBy(ks => ks.Name)
            .ToList();

        return Success(new ModuleKnowledgeStructuresDto
        {
            ModuleId = module.Id,
            ModuleName = module.Name,
            KnowledgeStructures = knowledgeStructures,
        });
    }
}
