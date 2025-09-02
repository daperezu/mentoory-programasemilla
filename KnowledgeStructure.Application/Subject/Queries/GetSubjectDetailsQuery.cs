using LinaSys.KnowledgeStructure.Application.Subject.DTOs;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Queries;

public sealed record GetSubjectDetailsQuery(long SubjectId) : IBaseRequest<SubjectDetailDto>;

public sealed class GetSubjectDetailsQueryHandler(
    ISubjectRepository subjectRepository,
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<GetSubjectDetailsQuery, SubjectDetailDto>
{
    public override async Task<Result<SubjectDetailDto>> Handle(
        GetSubjectDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetWithResourcesAsync(request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                ("Subject", $"Asignatura con ID {request.SubjectId} no encontrada"));
        }

        // Find the topic that references this subject
        var topics = await knowledgeStructureRepository.GetTopicsReferencingSubjectAsync(
            request.SubjectId,
            cancellationToken);

        var topic = topics.FirstOrDefault();
        Domain.Aggregates.KnowledgeStructure.KnowledgeStructureModule? module = null;
        Domain.Aggregates.KnowledgeStructure.KnowledgeStructure? structure = null;

        if (topic is not null)
        {
            module = await knowledgeStructureRepository.GetModuleWithStructureAsync(
                topic.KnowledgeStructureModuleId,
                cancellationToken);

            if (module is not null)
            {
                structure = await knowledgeStructureRepository.FindByIdAsync(
                    module.KnowledgeStructureId,
                    cancellationToken);
            }
        }

        var dto = new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectId = subject.Id,
            Title = subject.Title,
            Content = subject.Content,
            TopicId = topic?.Id ?? 0,
            TopicName = topic?.Topic.Name ?? "Sin tema asociado",
            ModuleId = module?.Id ?? 0,
            ModuleName = module?.Module.Name ?? "Sin módulo asociado",
            KnowledgeStructureId = structure?.Id ?? 0,
            KnowledgeStructureName = structure?.Name ?? "Sin estructura asociada",
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
