using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Commands;

public sealed record RemoveSubjectResourceCommand(
    long SubjectId,
    long ResourceId) : IBaseRequest;

public sealed class RemoveSubjectResourceCommandHandler(
    ISubjectRepository subjectRepository)
    : BaseCommandHandler<RemoveSubjectResourceCommand>
{
    public override async Task<Result> Handle(RemoveSubjectResourceCommand request, CancellationToken cancellationToken)
    {
        // Get the subject with resources
        var subject = await subjectRepository.GetWithResourcesByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.SubjectId), "No se encontró el tema especificado."));
        }

        // Find the resource
        var resource = subject.SubjectResources.FirstOrDefault(r => r.Id == request.ResourceId);
        if (resource is null)
        {
            return Failure(
                ResultErrorCodes.SubjectResource_NotFound,
                (nameof(request.ResourceId), "No se encontró el recurso especificado."));
        }

        // Remove the resource
        subject.RemoveResource(request.ResourceId);

        await subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
