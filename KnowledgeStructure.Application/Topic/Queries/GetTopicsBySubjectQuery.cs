using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Queries;

public sealed record GetTopicsBySubjectQuery(long SubjectId) : IBaseRequest<List<SimpleTopicDto>>;

public sealed record SimpleTopicDto(
    long Id,
    string Name,
    string? Description);

public sealed class GetTopicsBySubjectQueryHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository) : BaseCommandHandler<GetTopicsBySubjectQuery, List<SimpleTopicDto>>
{
    public override async Task<Result<List<SimpleTopicDto>>> Handle(
        GetTopicsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        // Get topics that reference this subject
        var structureTopics = await knowledgeStructureRepository.GetTopicsReferencingSubjectAsync(
            request.SubjectId,
            cancellationToken);

        var topics = structureTopics
            .Select(st => new SimpleTopicDto(
                st.Topic.Id,
                st.Topic.Name,
                st.Topic.Description))
            .Distinct()
            .ToList();

        return Success(topics);
    }
}