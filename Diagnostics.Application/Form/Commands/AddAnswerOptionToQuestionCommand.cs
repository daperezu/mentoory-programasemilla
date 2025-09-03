using FluentValidation;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Commands;

public sealed record AddAnswerOptionToQuestionCommand(
    long QuestionId,
    string Text,
    int Score,
    char Foda,
    string FodaExplanation,
    char Odsr,
    string OdsrExplanation,
    string? FollowUpQuestionText,
    int Order) : IBaseRequest;

public class AddAnswerOptionToQuestionCommandValidator : AbstractValidator<AddAnswerOptionToQuestionCommand>
{
    public AddAnswerOptionToQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Score).InclusiveBetween(0, 100);
        RuleFor(x => x.Foda)
            .Must(value => Enum.IsDefined(typeof(FodaType), (int)value))
            .WithMessage("FodaType must be a valid enum value.");
        RuleFor(x => x.Odsr)
            .Must(value => Enum.IsDefined(typeof(OdsrType), (int)value))
            .WithMessage("OdsrType must be a valid enum value.");
        RuleFor(x => x.FodaExplanation).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OdsrExplanation).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(1);
    }
}

public sealed class AddAnswerOptionToQuestionCommandHandler(IFormRepository repository)
    : BaseCommandHandler<AddAnswerOptionToQuestionCommand>
{
    public override async Task<Result> Handle(AddAnswerOptionToQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await repository.GetQuestionByIdWithAnswerOptions(request.QuestionId, cancellationToken);
        if (question is null)
        {
            return Failure(ResultErrorCodes.Question_NotFound, (nameof(request.QuestionId), "The question template was not found."));
        }

        question.AddAnswerOption(
            request.Text,
            request.Score,
            (FodaType)request.Foda,
            request.FodaExplanation,
            (OdsrType)request.Odsr,
            request.OdsrExplanation,
            request.FollowUpQuestionText,
            request.Order);

        return Success();
    }
}
