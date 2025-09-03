using LinaSys.KnowledgeStructure.Application.Topic.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Queries;

public sealed record ListTopicsQuery(
    int Start,
    int Length,
    string? GlobalSearch,
    string? Name,
    long? StructureModuleId,
    long? KnowledgeStructureId,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<TopicListDto>>;

public sealed class ListTopicsQueryHandler(
    IKnowledgeStructureRepository repository) : BaseCommandHandler<ListTopicsQuery, FilteredQueryResult<TopicListDto>>
{
    public override async Task<Result<FilteredQueryResult<TopicListDto>>> Handle(ListTopicsQuery request, CancellationToken cancellationToken)
    {
        // Get all topics from repository
        var allTopics = await repository.GetAllStructureTopicsAsync(cancellationToken);

        // Apply filters
        var filteredTopics = allTopics.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
        {
            var search = request.GlobalSearch.ToLower();
            filteredTopics = filteredTopics.Where(st =>
                st.Topic.Name.ToLower().Contains(search) ||
                st.KnowledgeStructureModule.Module.Name.ToLower().Contains(search) ||
                st.KnowledgeStructureModule.KnowledgeStructure.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filteredTopics = filteredTopics.Where(st => st.Topic.Name.ToLower().Contains(request.Name.ToLower()));
        }

        if (request.StructureModuleId.HasValue)
        {
            filteredTopics = filteredTopics.Where(st => st.KnowledgeStructureModuleId == request.StructureModuleId.Value);
        }

        if (request.KnowledgeStructureId.HasValue)
        {
            filteredTopics = filteredTopics.Where(st => st.KnowledgeStructureModule.KnowledgeStructureId == request.KnowledgeStructureId.Value);
        }

        // Load SubjectReferences for subject count
        var topicIds = filteredTopics.Select(t => t.Id).ToList();
        var topicsWithReferences = new List<Domain.Aggregates.KnowledgeStructure.KnowledgeStructureTopic>();

        foreach (var topicId in topicIds)
        {
            var topicWithRefs = await repository.GetStructureTopicWithSubjectsAsync(topicId, cancellationToken);
            if (topicWithRefs is not null)
            {
                topicsWithReferences.Add(topicWithRefs);
            }
        }

        // Get total count before pagination
        var total = topicsWithReferences.Count;

        // Apply sorting
        var isDescending = request.OrderDirection?.ToLower() == "desc";
        var sortedTopics = topicsWithReferences.AsQueryable();

        sortedTopics = request.OrderByColumn?.ToLower() switch
        {
            "name" => isDescending
                ? sortedTopics.OrderByDescending(st => st.Topic.Name)
                : sortedTopics.OrderBy(st => st.Topic.Name),
            "modulename" => isDescending
                ? sortedTopics.OrderByDescending(st => st.KnowledgeStructureModule.Module.Name)
                : sortedTopics.OrderBy(st => st.KnowledgeStructureModule.Module.Name),
            "knowledgestructurename" => isDescending
                ? sortedTopics.OrderByDescending(st => st.KnowledgeStructureModule.KnowledgeStructure.Name)
                : sortedTopics.OrderBy(st => st.KnowledgeStructureModule.KnowledgeStructure.Name),
            "subjectcount" => isDescending
                ? sortedTopics.OrderByDescending(st => st.SubjectReferences.Count)
                : sortedTopics.OrderBy(st => st.SubjectReferences.Count),
            _ => sortedTopics.OrderBy(st => st.Topic.Name),
        };

        // Apply pagination
        var paginatedData = sortedTopics
            .Skip(request.Start)
            .Take(request.Length)
            .Select(st => new TopicListDto(
                st.Id,
                st.TopicId,
                st.Topic.Name,
                st.Topic.Description,
                st.KnowledgeStructureModule.Module.Name,
                st.KnowledgeStructureModule.KnowledgeStructure.Name,
                st.SubjectReferences.Count))
            .ToList();

        var result = new FilteredQueryResult<TopicListDto>(
            total,
            total,
            paginatedData);

        return Success(result);
    }
}
