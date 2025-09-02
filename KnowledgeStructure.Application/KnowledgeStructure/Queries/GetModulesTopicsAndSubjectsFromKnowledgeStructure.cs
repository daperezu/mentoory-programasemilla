using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;

public sealed record GetModulesTopicsAndSubjectsFromKnowledgeStructure(long KnowledgeStructureId) : IBaseRequest<KnowledgeStructureDto>;

public sealed record KnowledgeStructureDto(long Id, string Name, string? Description, List<ModuleDto> Modules);

public sealed record ModuleDto(long Id, long StructureModuleId, string Name, int Order, List<TopicDto> Topics);

public sealed record TopicDto(long Id, long StructureTopicId, string Name, int Order, List<SubjectDto> Subjects);

public sealed record SubjectDto(long Id, string Title, string? Content, int Order, List<SubjectResourceDto> Resources);

public sealed record SubjectResourceDto(long Id, string Title, string Url, string Type, int? EstimatedMinutes, int Order);

internal class GetModulesAndTopicsFromKnowledgeStructureValidator : AbstractValidator<GetModulesTopicsAndSubjectsFromKnowledgeStructure>
{
    public GetModulesAndTopicsFromKnowledgeStructureValidator()
    {
        RuleFor(query => query.KnowledgeStructureId)
            .GreaterThan(0)
            .WithMessage("Knowledge Structure ID must be greater than 0.");
    }
}

internal class GetModulesAndTopicsFromKnowledgeStructureHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository) : BaseCommandHandler<GetModulesTopicsAndSubjectsFromKnowledgeStructure, KnowledgeStructureDto>
{
    public override async Task<Result<KnowledgeStructureDto>> Handle(GetModulesTopicsAndSubjectsFromKnowledgeStructure request, CancellationToken cancellationToken)
    {
        var knowledgeStructure = await knowledgeStructureRepository.GetWithModulesTopicsAndSubjectsByIdAsync(request.KnowledgeStructureId, cancellationToken).ConfigureAwait(false);

        if (knowledgeStructure is null)
        {
            return Failure(ResultErrorCodes.KnowledgeStructure_NotFound, (nameof(request.KnowledgeStructureId), "Knowledge Structure not found."));
        }

        // Get all subjects with their resources
        var allSubjects = await subjectRepository.GetAllWithResourcesAsync(cancellationToken);
        var subjectLookup = allSubjects.ToDictionary(s => s.Id);

        var moduleDtos = new List<ModuleDto>();

        foreach (var structureModule in knowledgeStructure.KnowledgeStructureModules.OrderBy(m => m.Order))
        {
            var topicDtos = new List<TopicDto>();

            foreach (var topic in structureModule.KnowledgeStructureTopics.OrderBy(t => t.Order))
            {
                var subjectDtos = new List<SubjectDto>();

                foreach (var subjectRef in topic.SubjectReferences.OrderBy(sr => sr.Order))
                {
                    if (subjectLookup.TryGetValue(subjectRef.SubjectId, out var subject))
                    {
                        var resourceDtos = subject.SubjectResources
                            .OrderBy(r => r.Order)
                            .Select(resource => new SubjectResourceDto(
                                resource.Id,
                                resource.Title,
                                resource.Url,
                                resource.Type,
                                resource.EstimatedMinutes,
                                resource.Order))
                            .ToList();

                        subjectDtos.Add(new SubjectDto(
                            subject.Id,
                            subject.Title,
                            subject.Content,
                            subjectRef.Order,
                            resourceDtos));
                    }
                }

                topicDtos.Add(new TopicDto(
                    topic.TopicId,
                    topic.Id,
                    topic.Topic.Name,
                    topic.Order,
                    subjectDtos));
            }

            moduleDtos.Add(new ModuleDto(
                structureModule.ModuleId,
                structureModule.Id,
                structureModule.Module.Name,
                structureModule.Order,
                topicDtos));
        }

        var knowledgeStructureDto = new KnowledgeStructureDto(
            knowledgeStructure.Id,
            knowledgeStructure.Name,
            knowledgeStructure.Description,
            moduleDtos);

        return Success(knowledgeStructureDto);
    }
}
