using FluentValidation;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Diagnostics.Application.Question.Commands;

/// <summary>
/// Command to create a new diagnostic question.
/// </summary>
public sealed record CreateQuestionCommand(
    string Text,
    AnswerType AnswerType,
    QuestionPhase AppliesToPhase,
    bool IsUsedForMentoringPlan,
    bool IsUsedForDiagnosis,
    long? TopicId,
    long? BlockId,
    List<AnswerOptionDto>? AnswerOptions) : IBaseRequest<long>;

/// <summary>
/// DTO for answer options.
/// </summary>
public sealed record AnswerOptionDto(
    string Text,
    int Score,
    FodaType Foda,
    string FodaExplanation,
    OdsrType Odsr,
    string OdsrExplanation,
    string? FollowupQuestionText,
    int Order);

/// <summary>
/// Validator for CreateQuestionCommand.
/// </summary>
public sealed class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("El texto de la pregunta es requerido.")
            .MaximumLength(500).WithMessage("El texto no puede exceder 500 caracteres.");

        RuleFor(x => x.AnswerType)
            .IsInEnum().WithMessage("El tipo de respuesta no es válido.");

        RuleFor(x => x.AppliesToPhase)
            .IsInEnum().WithMessage("La fase no es válida.");

        When(x => x.AnswerOptions is not null && x.AnswerOptions.Any(), () =>
        {
            RuleForEach(x => x.AnswerOptions).ChildRules(option =>
            {
                option.RuleFor(o => o.Text)
                    .NotEmpty().WithMessage("El texto de la opción es requerido.")
                    .MaximumLength(200).WithMessage("El texto de la opción no puede exceder 200 caracteres.");

                option.RuleFor(o => o.Foda)
                    .IsInEnum().WithMessage("El tipo FODA no es válido.");

                option.RuleFor(o => o.Odsr)
                    .IsInEnum().WithMessage("El tipo ODSR no es válido.");

                option.RuleFor(o => o.FodaExplanation)
                    .NotEmpty().WithMessage("La explicación FODA es requerida.")
                    .MaximumLength(500).WithMessage("La explicación FODA no puede exceder 500 caracteres.");

                option.RuleFor(o => o.OdsrExplanation)
                    .NotEmpty().WithMessage("La explicación ODSR es requerida.")
                    .MaximumLength(500).WithMessage("La explicación ODSR no puede exceder 500 caracteres.");

                option.RuleFor(o => o.FollowupQuestionText)
                    .MaximumLength(500).WithMessage("La pregunta de seguimiento no puede exceder 500 caracteres.");
            });
        });
    }
}

/// <summary>
/// Handler for CreateQuestionCommand.
/// </summary>
public sealed class CreateQuestionCommandHandler(
    IQuestionRepository questionRepository,
    DiagnosticsDbContext dbContext,
    ILogger<CreateQuestionCommandHandler> logger)
    : BaseCommandHandler<CreateQuestionCommand, long>
{
    public override async Task<Result<long>> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating new question: {Text}", request.Text);

            // Create the question
            var question = new Domain.Aggregates.Form.Question(
                request.Text,
                request.AnswerType,
                request.AppliesToPhase,
                request.IsUsedForMentoringPlan,
                request.IsUsedForDiagnosis);

            // Add answer options if provided
            if (request.AnswerOptions is not null && request.AnswerOptions.Any())
            {
                foreach (var optionDto in request.AnswerOptions.OrderBy(o => o.Order))
                {
                    question.AddAnswerOption(
                        optionDto.Text,
                        optionDto.Score,
                        optionDto.Foda,
                        optionDto.FodaExplanation,
                        optionDto.Odsr,
                        optionDto.OdsrExplanation,
                        optionDto.FollowupQuestionText,
                        optionDto.Order);
                }
            }

            // Add the question to the repository
            questionRepository.Add(question);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Question created successfully with ID: {QuestionId}", question.Id);
            return Success(question.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating question");
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(request.Text), "Error al crear la pregunta."));
        }
    }
}