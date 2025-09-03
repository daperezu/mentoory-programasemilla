using LinaSys.KnowledgeStructure.Application.Topic.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Queries;

public sealed record GetTopicDetailsQuery(long Id) : IBaseRequest<TopicDetailDto>;

public sealed class GetTopicDetailsQueryHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<GetTopicDetailsQuery, TopicDetailDto>
{
    public override async Task<Result<TopicDetailDto>> Handle(
        GetTopicDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var topic = await repository.GetTopicWithModuleAsync(request.Id, cancellationToken);
        if (topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                ("Topic", $"Tema con ID {request.Id} no encontrado"));
        }

        var module = await repository.GetModuleWithStructureAsync(
            topic.KnowledgeStructureModuleId,
            cancellationToken);

        var structure = module is not null
            ? await repository.FindByIdAsync(module.KnowledgeStructureId, cancellationToken)
            : null;

        var dto = new TopicDetailDto
        {
            Id = topic.Id,
            Name = topic.Topic.Name,
            Description = topic.Topic.Description,
            Order = topic.Order,
            ModuleId = topic.KnowledgeStructureModuleId,
            ModuleName = module?.Module.Name ?? "Módulo no encontrado",
            KnowledgeStructureId = module?.KnowledgeStructureId ?? 0,
            KnowledgeStructureName = structure?.Name ?? "Estructura no encontrada",
            SubjectCount = topic.SubjectReferences.Count,
        };

        return Success(dto);
    }
}
