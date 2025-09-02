using LinaSys.KnowledgeStructure.Application.Subject.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Queries;

public sealed record ListSubjectsQuery(
    int Start,
    int Length,
    string? GlobalSearch,
    string? Title,
    long? StructureTopicId,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<SubjectListDto>>;

public sealed class ListSubjectsQueryHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository)
    : BaseCommandHandler<ListSubjectsQuery, FilteredQueryResult<SubjectListDto>>
{
    public override async Task<Result<FilteredQueryResult<SubjectListDto>>> Handle(
        ListSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all topics with their subject references
        var allTopics = await knowledgeStructureRepository.GetAllStructureTopicsAsync(cancellationToken);

        // Get all subjects
        var allSubjects = await subjectRepository.GetAllWithResourcesAsync(cancellationToken);

        // Create a lookup for subjects
        var subjectLookup = allSubjects.ToDictionary(s => s.Id);

        // Join topics with subjects through SubjectReferences
        var subjectDtos = new List<SubjectListDto>();

        foreach (var topic in allTopics)
        {
            foreach (var subjectRef in topic.SubjectReferences)
            {
                if (subjectLookup.TryGetValue(subjectRef.SubjectId, out var subject))
                {
                    subjectDtos.Add(new SubjectListDto
                    {
                        Id = subjectRef.SubjectId, // Using SubjectId as Id for compatibility
                        SubjectId = subjectRef.SubjectId,
                        Title = subject.Title,
                        Content = subject.Content,
                        TopicName = topic.Topic.Name,
                        ModuleName = topic.KnowledgeStructureModule.Module.Name,
                        KnowledgeStructureName = topic.KnowledgeStructureModule.KnowledgeStructure.Name,
                        ResourceCount = subject.SubjectResources.Count,
                        Order = subjectRef.Order,
                    });
                }
            }
        }

        // Apply filters
        var filteredSubjects = subjectDtos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
        {
            var search = request.GlobalSearch.ToLower();
            filteredSubjects = filteredSubjects.Where(s =>
                s.Title.ToLower().Contains(search) ||
                (s.Content != null && s.Content.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            filteredSubjects = filteredSubjects.Where(s => s.Title.ToLower().Contains(request.Title.ToLower()));
        }

        if (request.StructureTopicId.HasValue)
        {
            // Find the topic and filter by its subject references
            var topic = allTopics.FirstOrDefault(t => t.Id == request.StructureTopicId.Value);
            if (topic is not null)
            {
                var subjectIds = topic.SubjectReferences.Select(sr => sr.SubjectId).ToHashSet();
                filteredSubjects = filteredSubjects.Where(s => subjectIds.Contains(s.SubjectId));
            }
        }

        // Get total count before pagination
        var totalRecords = filteredSubjects.Count();

        // Apply sorting
        var isDescending = request.OrderDirection?.ToLower() == "desc";
        filteredSubjects = request.OrderByColumn?.ToLower() switch
        {
            "title" => isDescending
                ? filteredSubjects.OrderByDescending(s => s.Title)
                : filteredSubjects.OrderBy(s => s.Title),
            "topicname" => isDescending
                ? filteredSubjects.OrderByDescending(s => s.TopicName)
                : filteredSubjects.OrderBy(s => s.TopicName),
            "resourcecount" => isDescending
                ? filteredSubjects.OrderByDescending(s => s.ResourceCount)
                : filteredSubjects.OrderBy(s => s.ResourceCount),
            _ => filteredSubjects.OrderBy(s => s.Order).ThenBy(s => s.Title),
        };

        // Apply pagination
        var subjects = filteredSubjects
            .Skip(request.Start)
            .Take(request.Length)
            .ToList();

        return Success(new FilteredQueryResult<SubjectListDto>(
            totalRecords,
            totalRecords,
            subjects));
    }
}
