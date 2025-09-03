using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Subject.Commands;

public sealed record CreateSubjectCommand(
    string Title,
    string? Content) : IBaseRequest<long>;

public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.Content)
            .MaximumLength(2000).WithMessage("El contenido no puede exceder 2000 caracteres.");
    }
}

public sealed class CreateSubjectCommandHandler(ISubjectRepository subjectRepository)
    : BaseCommandHandler<CreateSubjectCommand, long>
{
    public override async Task<Result<long>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        // Check if title already exists
        var isTitleTaken = await subjectRepository.IsTitleTakenAsync(request.Title, cancellationToken: cancellationToken);
        if (isTitleTaken)
        {
            return Failure(
                ResultErrorCodes.Subject_TitleAlreadyExists,
                (nameof(request.Title), "Ya existe un tema con este título."));
        }

        // Create the subject as its own aggregate
        var subject = new Domain.Aggregates.Subject.Subject(request.Title, request.Content);
        subjectRepository.Add(subject);

        // Save to get the subject ID
        await subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(subject.Id);
    }
}
