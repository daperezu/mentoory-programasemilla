using LinaSys.KnowledgeStructure.Domain.Aggregates.Subject;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Commands;

public sealed record AddSubjectResourceCommand(
    long SubjectId,
    string Title,
    string Url,
    string Type,
    int? EstimatedMinutes) : IBaseRequest<long>;

public sealed class AddSubjectResourceCommandHandler(
    ISubjectRepository subjectRepository)
    : BaseCommandHandler<AddSubjectResourceCommand, long>
{
    public override async Task<Result<long>> Handle(AddSubjectResourceCommand request, CancellationToken cancellationToken)
    {
        // Get the subject with resources
        var subject = await subjectRepository.GetWithResourcesByIdAsync(request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.SubjectId), "No se encontró el tema especificado."));
        }

        // Add the resource
        subject.AddResource(request.Title, request.Url, request.Type, request.EstimatedMinutes);

        await subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get the newly added resource to return its ID
        var newResource = subject.SubjectResources.OrderByDescending(r => r.Order).FirstOrDefault();
        return Success(newResource?.Id ?? 0);
    }
}
