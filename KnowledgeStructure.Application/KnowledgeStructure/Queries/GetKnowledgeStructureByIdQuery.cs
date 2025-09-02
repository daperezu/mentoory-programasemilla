using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;

public sealed record GetKnowledgeStructureByIdQuery(long Id) : IBaseRequest<KnowledgeStructureDetailDto>;

public sealed class GetKnowledgeStructureByIdQueryHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<GetKnowledgeStructureByIdQuery, KnowledgeStructureDetailDto>
{
    public override async Task<Result<KnowledgeStructureDetailDto>> Handle(
        GetKnowledgeStructureByIdQuery request,
        CancellationToken cancellationToken)
    {
        var structure = await repository.GetWithModulesAsync(request.Id, cancellationToken);
        if (structure is null)
        {
            return Failure(
                ResultErrorCodes.KnowledgeStructure_NotFound,
                ("KnowledgeStructure", $"Estructura de conocimiento con ID {request.Id} no encontrada"));
        }

        var dto = new KnowledgeStructureDetailDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            IsActive = structure.IsActive,
            ModuleCount = structure.KnowledgeStructureModules.Count,
            CreatedAt = structure.CreatedAt,
        };

        return Success(dto);
    }
}
