using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Commands;

public sealed record DeleteSubjectCommand(long SubjectId) : IBaseRequest;

public sealed class DeleteSubjectCommandHandler(ISubjectRepository subjectRepository)
    : BaseCommandHandler<DeleteSubjectCommand>
{
    public override async Task<Result> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        // Get the subject
        var subject = await subjectRepository.GetWithResourcesByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.SubjectId), "No se encontró el tema especificado."));
        }

        // Check if subject has resources (business rule validation)
        if (subject.SubjectResources.Count > 0)
        {
            return Failure(
                ResultErrorCodes.Subject_CannotDeleteWithResources,
                (nameof(request.SubjectId), "No se puede eliminar un tema que tiene recursos asociados."));
        }

        // Delete the subject (single aggregate operation)
        subjectRepository.Remove(subject);

        // Save changes for this aggregate only
        await subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
