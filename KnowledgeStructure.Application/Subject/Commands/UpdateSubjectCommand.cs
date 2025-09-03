using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Commands;

public sealed record UpdateSubjectCommand(
    long Id,
    string Title,
    string? Content) : IBaseRequest;

public sealed class UpdateSubjectCommandHandler(
    ISubjectRepository subjectRepository)
    : BaseCommandHandler<UpdateSubjectCommand>
{
    public override async Task<Result> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        // Get the subject directly as it's its own aggregate
        var subject = await subjectRepository.FindByIdAsync(request.Id, cancellationToken);

        if (subject is null)
        {
            return Failure(
                ResultErrorCodes.Subject_NotFound,
                (nameof(request.Id), "No se encontró el tema especificado."));
        }

        // Check if new title already exists (excluding current subject)
        var isTitleTaken = await subjectRepository.IsTitleTakenAsync(
            request.Title,
            excludingId: request.Id,
            cancellationToken: cancellationToken);

        if (isTitleTaken)
        {
            return Failure(
                ResultErrorCodes.Subject_TitleAlreadyExists,
                (nameof(request.Title), "Ya existe un tema con este título."));
        }

        // Update the subject
        subject.Update(request.Title, request.Content);

        await subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
