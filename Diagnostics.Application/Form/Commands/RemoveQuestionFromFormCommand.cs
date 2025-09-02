using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Commands;

public sealed record RemoveQuestionFromFormCommand(long FormId, long QuestionId) : IBaseRequest;

public class RemoveQuestionFromFormCommandValidator : AbstractValidator<RemoveQuestionFromFormCommand>
{
    public RemoveQuestionFromFormCommandValidator()
    {
        RuleFor(x => x.FormId)
            .GreaterThan(0).WithMessage("Form ID must be greater than 0.");

        RuleFor(x => x.QuestionId)
            .GreaterThan(0).WithMessage("Question ID must be greater than 0.");
    }
}

public sealed class RemoveQuestionFromFormCommandHandler(IFormRepository repository)
    : BaseCommandHandler<RemoveQuestionFromFormCommand>
{
    public override async Task<Result> Handle(RemoveQuestionFromFormCommand request, CancellationToken cancellationToken)
    {
        var form = await repository.GetByIdWithQuestions(request.FormId, cancellationToken);
        if (form is null)
        {
            return Failure(ResultErrorCodes.DiagnosisForm_NotFound, (nameof(request.FormId), "Formulario no encontrado."));
        }

        try
        {
            form.RemoveQuestion(request.QuestionId);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return Success();
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.DiagnosisForm_QuestionNotFound, (nameof(request.QuestionId), ex.Message));
        }
    }
}
