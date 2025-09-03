using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetModuleByIdQuery(long ModuleId) : IBaseRequest<ModuleDetailsDto>;

public sealed record ModuleDetailsDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long KnowledgeStructureId { get; set; }

    public string KnowledgeStructureName { get; set; } = string.Empty;

    public int Order { get; set; }
}

public sealed class GetModuleByIdQueryHandler(
    IModuleRepository moduleRepository,
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<GetModuleByIdQuery, ModuleDetailsDto>
{
    public override async Task<Result<ModuleDetailsDto>> Handle(
        GetModuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);

        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.ModuleId), "El módulo no existe."));
        }

        // Get the knowledge structure that contains this module
        var structures = await knowledgeStructureRepository.GetAllWithModulesAsync(cancellationToken);

        var moduleDetails = (from ks in structures
                           from ksm in ks.KnowledgeStructureModules
                           where ksm.ModuleId == module.Id
                           select new ModuleDetailsDto
                           {
                               Id = module.Id,
                               Name = module.Name,
                               Description = null, // KnowledgeStructureModule doesn't have Description
                               KnowledgeStructureId = ks.Id,
                               KnowledgeStructureName = ks.Name,
                               Order = ksm.Order,
                           }).FirstOrDefault();

        if (moduleDetails is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.ModuleId), "El módulo no está asociado a ninguna estructura de conocimiento."));
        }

        return Success(moduleDetails);
    }
}
