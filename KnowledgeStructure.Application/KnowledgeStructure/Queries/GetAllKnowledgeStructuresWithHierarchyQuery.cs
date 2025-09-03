using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;

public sealed record GetAllKnowledgeStructuresWithHierarchyQuery : IBaseRequest<List<KnowledgeStructureHierarchyDto>>;

public sealed record KnowledgeStructureHierarchyDto(
    long Id,
    string Name,
    bool IsActive,
    List<ModuleHierarchyDto> Modules);

public sealed record ModuleHierarchyDto(
    long Id,
    long StructureModuleId,
    string Name,
    long KnowledgeStructureId,
    string KnowledgeStructureName,
    List<TopicHierarchyDto> Topics);

public sealed record TopicHierarchyDto(
    long Id,
    string Name,
    long ModuleId,
    string ModuleName,
    long KnowledgeStructureId,
    string KnowledgeStructureName,
    List<SubjectHierarchyDto> Subjects);

public sealed record SubjectHierarchyDto(
    long Id,
    string Title,
    long TopicId,
    string TopicName,
    long ModuleId,
    string ModuleName,
    long KnowledgeStructureId,
    string KnowledgeStructureName);

internal class GetAllKnowledgeStructuresWithHierarchyQueryHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository) : BaseCommandHandler<GetAllKnowledgeStructuresWithHierarchyQuery, List<KnowledgeStructureHierarchyDto>>
{
    public override async Task<Result<List<KnowledgeStructureHierarchyDto>>> Handle(
        GetAllKnowledgeStructuresWithHierarchyQuery request,
        CancellationToken cancellationToken)
    {
        // Since we don't have a single method for full hierarchy, we'll build it step by step
        var knowledgeStructures = await knowledgeStructureRepository.ListAllActiveAsync(cancellationToken);
        var result = new List<KnowledgeStructureHierarchyDto>();

        foreach (var ks in knowledgeStructures)
        {
            // Get the full hierarchy for this knowledge structure
            var fullStructure = await knowledgeStructureRepository.GetWithModulesTopicsAndSubjectsByIdAsync(ks.Id, cancellationToken);
            if (fullStructure is null)
            {
                continue;
            }

            var allSubjects = await subjectRepository.ListAllAsync(cancellationToken);
            var subjectLookup = allSubjects.ToDictionary(s => s.Id);

            var modules = new List<ModuleHierarchyDto>();

            foreach (var structureModule in fullStructure.KnowledgeStructureModules.OrderBy(m => m.Order))
            {
                var topics = new List<TopicHierarchyDto>();

                foreach (var topic in structureModule.KnowledgeStructureTopics.OrderBy(t => t.Order))
                {
                    var subjects = new List<SubjectHierarchyDto>();

                    foreach (var subjectRef in topic.SubjectReferences.OrderBy(sr => sr.Order))
                    {
                        if (subjectLookup.TryGetValue(subjectRef.SubjectId, out var subject))
                        {
                            subjects.Add(new SubjectHierarchyDto(
                                subject.Id,
                                subject.Title,
                                topic.TopicId,
                                topic.Topic.Name,
                                structureModule.ModuleId,
                                structureModule.Module.Name,
                                ks.Id,
                                ks.Name));
                        }
                    }

                    topics.Add(new TopicHierarchyDto(
                        topic.TopicId,
                        topic.Topic.Name,
                        structureModule.ModuleId,
                        structureModule.Module.Name,
                        ks.Id,
                        ks.Name,
                        subjects));
                }

                modules.Add(new ModuleHierarchyDto(
                    structureModule.ModuleId,
                    structureModule.Id,
                    structureModule.Module.Name,
                    ks.Id,
                    ks.Name,
                    topics));
            }

            result.Add(new KnowledgeStructureHierarchyDto(
                ks.Id,
                ks.Name,
                ks.IsActive,
                modules));
        }

        return Success(result);
    }
}
