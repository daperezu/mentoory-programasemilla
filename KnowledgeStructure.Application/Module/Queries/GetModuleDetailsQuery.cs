using LinaSys.KnowledgeStructure.Application.Module.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Queries;

public sealed record GetModuleDetailsQuery(long Id) : IBaseRequest<ModuleDetailDto>;

public sealed class GetModuleDetailsQueryHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<GetModuleDetailsQuery, ModuleDetailDto>
{
    public override async Task<Result<ModuleDetailDto>> Handle(
        GetModuleDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var module = await repository.GetModuleWithStructureAsync(request.Id, cancellationToken);
        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                ("Module", $"Módulo con ID {request.Id} no encontrado"));
        }

        var structure = await repository.FindByIdAsync(module.KnowledgeStructureId, cancellationToken);

        var dto = new ModuleDetailDto
        {
            Id = module.Id,
            Name = module.Module.Name,
            Description = null, // Module entity doesn't have Description
            Order = module.Order,
            KnowledgeStructureId = module.KnowledgeStructureId,
            KnowledgeStructureName = structure?.Name ?? "Estructura no encontrada",
            TopicCount = module.KnowledgeStructureTopics.Count,
        };

        return Success(dto);
    }
}
