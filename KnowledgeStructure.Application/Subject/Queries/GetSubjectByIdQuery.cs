using LinaSys.KnowledgeStructure.Application.Subject.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Queries;

public sealed record GetSubjectByIdQuery(long Id) : IBaseRequest<SubjectDetailDto>;

public sealed class GetSubjectByIdQueryHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository)
    : BaseCommandHandler<GetSubjectByIdQuery, SubjectDetailDto>
{
    public override async Task<Result<SubjectDetailDto>> Handle(
        GetSubjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get the subject directly
        var subject = await subjectRepository.GetWithResourcesByIdAsync(request.Id, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.Id), "No se encontró el tema especificado."));
        }

        // Find which topic references this subject
        var topics = await knowledgeStructureRepository.GetTopicsReferencingSubjectAsync(request.Id, cancellationToken);
        var topic = topics.FirstOrDefault();

        if (topic is null)
        {
            // Subject exists but is not referenced by any topic
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.Id), "El tema no está asociado a ningún tópico."));
        }

        // Load the full topic structure
        var fullTopic = await knowledgeStructureRepository.GetStructureTopicByIdAsync(topic.Id, cancellationToken);
        if (fullTopic is null)
        {
            return Failure(
                ResultErrorCodes.Topic_NotFound,
                (nameof(topic.Id), "No se encontró el tópico."));
        }

        var subjectRef = fullTopic.SubjectReferences.FirstOrDefault(sr => sr.SubjectId == request.Id);

        var dto = new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectId = subject.Id,
            Title = subject.Title,
            Content = subject.Content,
            TopicId = fullTopic.Id,
            TopicName = fullTopic.Topic.Name,
            ModuleId = fullTopic.KnowledgeStructureModule.Module.Id,
            ModuleName = fullTopic.KnowledgeStructureModule.Module.Name,
            KnowledgeStructureId = fullTopic.KnowledgeStructureModule.KnowledgeStructure.Id,
            KnowledgeStructureName = fullTopic.KnowledgeStructureModule.KnowledgeStructure.Name,
            Resources = subject.SubjectResources
                .OrderBy(r => r.Order)
                .Select(r => new SubjectResourceDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Url = r.Url,
                    Type = r.Type,
                    EstimatedMinutes = r.EstimatedMinutes,
                    Order = r.Order,
                })
                .ToList(),
        };

        return Success(dto);
    }
}
