using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Queries;

public sealed record GetTopicModulesQuery(long TopicId) : IBaseRequest<TopicModulesDto>;

public sealed record TopicModulesDto
{
    public long TopicId { get; init; }

    public string TopicName { get; init; } = string.Empty;

    public List<ModuleAssignmentDto> Modules { get; init; } = [];
}

public sealed record ModuleAssignmentDto
{
    public long StructureTopicId { get; init; }

    public long StructureModuleId { get; init; }

    public string ModuleName { get; init; } = string.Empty;

    public string KnowledgeStructureName { get; init; } = string.Empty;

    public int Order { get; init; }

    public int SubjectCount { get; init; }
}

public sealed class GetTopicModulesQueryValidator : AbstractValidator<GetTopicModulesQuery>
{
    public GetTopicModulesQueryValidator()
    {
        RuleFor(x => x.TopicId)
            .GreaterThan(0).WithMessage("El ID del tema debe ser mayor a 0.");
    }
}

public sealed class GetTopicModulesQueryHandler(
    ITopicRepository topicRepository,
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<GetTopicModulesQuery, TopicModulesDto>
{
    public override async Task<Result<TopicModulesDto>> Handle(
        GetTopicModulesQuery request,
        CancellationToken cancellationToken)
    {
        // Get the topic
        var topic = await topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(request.TopicId), "El tema no existe."));
        }

        // Get all structure topics for this topic
        var allStructureTopics = await knowledgeStructureRepository.GetAllStructureTopicsAsync(cancellationToken);
        var topicModules = allStructureTopics
            .Where(st => st.TopicId == request.TopicId)
            .ToList();

        // Get subject counts for each structure topic
        var modules = new List<ModuleAssignmentDto>();
        foreach (var structureTopic in topicModules)
        {
            var topicWithRefs = await knowledgeStructureRepository.GetStructureTopicWithSubjectsAsync(
                structureTopic.Id,
                cancellationToken);

            modules.Add(new ModuleAssignmentDto
            {
                StructureTopicId = structureTopic.Id,
                StructureModuleId = structureTopic.KnowledgeStructureModuleId,
                ModuleName = structureTopic.KnowledgeStructureModule.Module.Name,
                KnowledgeStructureName = structureTopic.KnowledgeStructureModule.KnowledgeStructure.Name,
                Order = structureTopic.Order,
                SubjectCount = topicWithRefs?.SubjectReferences.Count ?? 0,
            });
        }

        return Success(new TopicModulesDto
        {
            TopicId = topic.Id,
            TopicName = topic.Name,
            Modules = modules.OrderBy(m => m.KnowledgeStructureName).ThenBy(m => m.ModuleName).ToList(),
        });
    }
}
