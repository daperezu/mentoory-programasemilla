using LinaSys.KnowledgeStructure.Application.Subject.Commands;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Orchestration.Application.KnowledgeStructure.Commands;

public sealed record DeleteSubjectOrchestrationCommand(long SubjectId) : LinaSys.Shared.Application.MediatR.IBaseRequest;

public sealed class DeleteSubjectOrchestrationCommandHandler(
    IMediator mediator,
    IKnowledgeStructureRepository knowledgeStructureRepository,
    ISubjectRepository subjectRepository) : BaseCommandHandler<DeleteSubjectOrchestrationCommand>
{
    public override async Task<Result> Handle(DeleteSubjectOrchestrationCommand request, CancellationToken cancellationToken)
    {
        // Get the subject first to validate it exists
        var subject = await subjectRepository.GetWithResourcesByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.SubjectId), "No se encontró el tema especificado."));
        }

        // Check if subject has resources
        if (subject.SubjectResources.Count > 0)
        {
            return Failure(
                ResultErrorCodes.Subject_CannotDeleteWithResources,
                (nameof(request.SubjectId), "No se puede eliminar un tema que tiene recursos asociados."));
        }

        // Find all topics that reference this subject
        var topics = await knowledgeStructureRepository.GetTopicsReferencingSubjectAsync(request.SubjectId, cancellationToken);

        // Remove references from all topics (KnowledgeStructure aggregate)
        foreach (var topic in topics)
        {
            topic.RemoveSubjectReference(request.SubjectId);
            await knowledgeStructureRepository.UpdateTopicAsync(topic, cancellationToken);
        }

        // Delete the subject through its own aggregate
        var deleteSubjectCommand = new DeleteSubjectCommand(request.SubjectId);
        var deleteResult = await mediator.Send(deleteSubjectCommand, cancellationToken);

        if (!deleteResult.IsSuccess)
        {
            return Failure(deleteResult.ErrorCode ?? ResultErrorCodes.Unknown, deleteResult.ErrorMessages ?? []);
        }

        return Success();
    }
}
