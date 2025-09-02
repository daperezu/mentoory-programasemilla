using LinaSys.KnowledgeStructure.Application.Topic.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Queries;

public sealed record GetTopicByIdQuery(long StructureTopicId) : IBaseRequest<TopicDto>;

public sealed class GetTopicByIdQueryHandler(
    IKnowledgeStructureRepository repository) : BaseCommandHandler<GetTopicByIdQuery, TopicDto>
{
    public override async Task<Result<TopicDto>> Handle(GetTopicByIdQuery request, CancellationToken cancellationToken)
    {
        // First get the topic with module to get the module ID
        var structureTopic = await repository.GetTopicWithModuleAsync(request.StructureTopicId, cancellationToken);

        if (structureTopic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.StructureTopicId), "El tema no existe."));
        }

        // Now get the full module with structure to get all the names
        var module = await repository.GetModuleWithStructureAsync(structureTopic.KnowledgeStructureModuleId, cancellationToken);

        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                ("Module", "El módulo no existe."));
        }

        var dto = new TopicDto(
            structureTopic.Id,
            structureTopic.TopicId,
            structureTopic.Topic.Name,
            structureTopic.Topic.Description,
            structureTopic.KnowledgeStructureModuleId,
            module.Module.Name,
            module.KnowledgeStructureId,
            module.KnowledgeStructure.Name);

        return Success(dto);
    }
}
