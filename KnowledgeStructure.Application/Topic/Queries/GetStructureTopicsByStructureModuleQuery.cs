using LinaSys.KnowledgeStructure.Application.Topic.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Queries;

public sealed record GetStructureTopicsByStructureModuleQuery(long StructureModuleId) : IBaseRequest<List<StructureTopicDto>>;

public sealed class GetStructureTopicsByStructureModuleQueryHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<GetStructureTopicsByStructureModuleQuery, List<StructureTopicDto>>
{
    public override async Task<Result<List<StructureTopicDto>>> Handle(
        GetStructureTopicsByStructureModuleQuery request,
        CancellationToken cancellationToken)
    {
        var structureModule = await knowledgeStructureRepository.GetStructureModuleWithTopicsAsync(
            request.StructureModuleId,
            cancellationToken);

        if (structureModule is null)
        {
            return Success([]);
        }

        var topics = structureModule.KnowledgeStructureTopics
            .OrderBy(t => t.Order)
            .Select(t => new StructureTopicDto
            {
                StructureTopicId = t.Id,
                TopicName = t.Topic.Name,
            })
            .ToList();

        return Success(topics);
    }
}
