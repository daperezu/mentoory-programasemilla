using FluentValidation;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Commands;

public sealed record AddQuestionToFormCommand(
    long FormId,
    long? TopicId,
    long BlockId,
    string Text,
    int AnswerType,
    int AppliesToPhase,
    bool IsUsedForMentoringPlan,
    bool IsUsedForDiagnosis,
    int Order) : IBaseRequest;

public class AddQuestionToFormCommandValidator : AbstractValidator<AddQuestionToFormCommand>
{
    public AddQuestionToFormCommandValidator()
    {
        RuleFor(x => x.FormId)
            .GreaterThan(0).WithMessage("FormId must be greater than 0.");

        When(x => x.TopicId.HasValue, () =>
        {
            RuleFor(x => x.TopicId!.Value)
                .GreaterThan(0).WithMessage("TopicId must be greater than 0.");
        });

        RuleFor(x => x.BlockId)
            .GreaterThan(0).WithMessage("BlockId must be greater than 0.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MaximumLength(500).WithMessage("Text must not exceed 500 characters.");

        RuleFor(x => x.AnswerType)
            .Must(value => Enum.IsDefined(typeof(AnswerType), value))
            .WithMessage("AnswerType must be a valid enum value.");

        RuleFor(x => x.AppliesToPhase)
            .Must(value => Enum.IsDefined(typeof(QuestionPhase), value))
            .WithMessage("AppliesToPhase must be a valid enum value.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be greater than or equal to 0.");
    }
}

public sealed class AddQuestionToFormCommandHandler(IFormRepository repository)
    : BaseCommandHandler<AddQuestionToFormCommand>
{
    public override async Task<Result> Handle(AddQuestionToFormCommand request, CancellationToken cancellationToken)
    {
        var form = await repository.GetByIdWithQuestions(request.FormId, cancellationToken);
        if (form is null)
        {
            return Failure(ResultErrorCodes.DiagnosisForm_NotFound, (nameof(request.FormId), "The shared form was not found."));
        }

        form.AddQuestion(
            request.TopicId,
            request.BlockId,
            request.Text,
            (AnswerType)request.AnswerType,
            (QuestionPhase)request.AppliesToPhase,
            request.IsUsedForMentoringPlan,
            request.IsUsedForDiagnosis,
            request.Order);

        return Success();
    }
}
