using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Topic.Queries;

public sealed record GetTopicWithSubjectsQuery(long TopicId) : IBaseRequest<TopicWithSubjectsDto>;

public sealed record TopicWithSubjectsDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<TopicSubjectDto> Subjects { get; set; } = [];
}

public sealed record TopicSubjectDto
{
    public long SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }
}

public sealed class GetTopicWithSubjectsQueryHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository)
    : BaseCommandHandler<GetTopicWithSubjectsQuery, TopicWithSubjectsDto>
{
    public override async Task<Result<TopicWithSubjectsDto>> Handle(
        GetTopicWithSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        var topic = await knowledgeStructureRepository.GetTopicWithSubjectReferencesAsync(
            request.TopicId,
            cancellationToken);

        if (topic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                ("Topic", $"Tema con ID {request.TopicId} no encontrado"));
        }

        var subjectIds = topic.SubjectReferences.Select(sr => sr.SubjectId).ToList();
        var subjects = await subjectRepository.GetByIdsAsync(subjectIds, cancellationToken);

        var dto = new TopicWithSubjectsDto
        {
            Id = topic.Id,
            Name = topic.Topic.Name,
            Subjects = topic.SubjectReferences
                .OrderBy(sr => sr.Order)
                .Select(sr =>
                {
                    var subject = subjects.FirstOrDefault(s => s.Id == sr.SubjectId);
                    return new TopicSubjectDto
                    {
                        SubjectId = sr.SubjectId,
                        Title = subject?.Title ?? "Asignatura no encontrada",
                        Order = sr.Order,
                    };
                })
                .ToList(),
        };

        return Success(dto);
    }
}
