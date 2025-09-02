using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Queries;

public sealed record GetSubjectTopicsQuery(long SubjectId) : IBaseRequest<SubjectTopicsDto>;

public sealed record SubjectTopicsDto
{
    public long SubjectId { get; init; }

    public string SubjectName { get; init; } = string.Empty;

    public string? SubjectDescription { get; init; }

    public List<TopicReferenceDto> Topics { get; init; } = [];
}

public sealed record TopicReferenceDto
{
    public long StructureTopicId { get; init; }

    public string TopicName { get; init; } = string.Empty;

    public string ModuleName { get; init; } = string.Empty;

    public string KnowledgeStructureName { get; init; } = string.Empty;

    public int Order { get; init; }
}

public sealed class GetSubjectTopicsQueryValidator : AbstractValidator<GetSubjectTopicsQuery>
{
    public GetSubjectTopicsQueryValidator()
    {
        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("El ID de la materia debe ser mayor a 0.");
    }
}

public sealed class GetSubjectTopicsQueryHandler(
    ISubjectRepository subjectRepository,
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<GetSubjectTopicsQuery, SubjectTopicsDto>
{
    public override async Task<Result<SubjectTopicsDto>> Handle(
        GetSubjectTopicsQuery request,
        CancellationToken cancellationToken)
    {
        // Get the subject
        var subject = await subjectRepository.FindByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.SubjectId), "La materia no existe."));
        }

        // Get all topics that reference this subject
        var topicsWithSubject = await knowledgeStructureRepository.GetTopicsReferencingSubjectAsync(
            request.SubjectId,
            cancellationToken);

        var topics = new List<TopicReferenceDto>();
        foreach (var structureTopic in topicsWithSubject)
        {
            // Get full topic details with module and knowledge structure
            var fullTopic = await knowledgeStructureRepository.GetStructureTopicByIdAsync(
                structureTopic.Id,
                cancellationToken);

            if (fullTopic is not null)
            {
                var subjectRef = structureTopic.SubjectReferences
                    .First(sr => sr.SubjectId == request.SubjectId);

                topics.Add(new TopicReferenceDto
                {
                    StructureTopicId = fullTopic.Id,
                    TopicName = fullTopic.Topic.Name,
                    ModuleName = fullTopic.KnowledgeStructureModule.Module.Name,
                    KnowledgeStructureName = fullTopic.KnowledgeStructureModule.KnowledgeStructure?.Name ?? string.Empty,
                    Order = subjectRef.Order,
                });
            }
        }

        return Success(new SubjectTopicsDto
        {
            SubjectId = subject.Id,
            SubjectName = subject.Title,
            SubjectDescription = subject.Content,
            Topics = topics
                .OrderBy(t => t.KnowledgeStructureName)
                .ThenBy(t => t.ModuleName)
                .ThenBy(t => t.TopicName)
                .ToList(),
        });
    }
}
