using FluentValidation;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Diagnostics.Application.Question.Commands;

/// <summary>
/// Command to update an existing diagnostic question.
/// </summary>
public sealed record UpdateQuestionCommand(
    long Id,
    string Text,
    AnswerType AnswerType,
    QuestionPhase AppliesToPhase,
    bool IsUsedForMentoringPlan,
    bool IsUsedForDiagnosis,
    List<UpdateAnswerOptionDto>? AnswerOptions) : IBaseRequest;

/// <summary>
/// DTO for updating answer options.
/// </summary>
public sealed record UpdateAnswerOptionDto(
    long? Id,
    string Text,
    int Score,
    FodaType Foda,
    string FodaExplanation,
    OdsrType Odsr,
    string OdsrExplanation,
    string? FollowupQuestionText,
    int Order,
    bool IsDeleted = false);

/// <summary>
/// Validator for UpdateQuestionCommand.
/// </summary>
public sealed class UpdateQuestionCommandValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la pregunta debe ser válido.");

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
/// Handler for UpdateQuestionCommand.
/// </summary>
public sealed class UpdateQuestionCommandHandler(
    IQuestionRepository questionRepository,
    DiagnosticsDbContext dbContext,
    ILogger<UpdateQuestionCommandHandler> logger)
    : BaseCommandHandler<UpdateQuestionCommand>
{
    public override async Task<Result> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating question with ID: {QuestionId}", request.Id);

            // Get the existing question
            var question = await questionRepository.GetByIdWithAnswerOptionsAsync(request.Id, cancellationToken);
            if (question is null)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(request.Id), "La pregunta no existe."));
            }

            // Update the question properties
            question.Update(
                request.Text,
                request.AnswerType,
                request.AppliesToPhase,
                request.IsUsedForMentoringPlan,
                request.IsUsedForDiagnosis);

            // Handle answer options if the answer type supports them
            if (request.AnswerType == AnswerType.SingleChoice ||
                request.AnswerType == AnswerType.MultiChoice)
            {
                // Process answer options updates
                if (request.AnswerOptions is not null)
                {
                    // Remove deleted options
                    var optionsToRemove = question.AnswerOptions
                        .Where(o => !request.AnswerOptions.Any(dto => dto.Id == o.Id))
                        .ToList();

                    foreach (var option in optionsToRemove)
                    {
                        question.RemoveAnswerOptionById(option.Id);
                    }

                    // Update existing and add new options
                    foreach (var optionDto in request.AnswerOptions.OrderBy(o => o.Order))
                    {
                        if (optionDto.Id.HasValue)
                        {
                            // Update existing option
                            var existingOption = question.AnswerOptions
                                .FirstOrDefault(o => o.Id == optionDto.Id.Value);

                            if (existingOption is not null)
                            {
                                existingOption.Update(
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
                        else
                        {
                            // Add new option
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
                }
            }
            else
            {
                // Remove all answer options if the answer type doesn't support them
                var allOptions = question.AnswerOptions.ToList();
                foreach (var option in allOptions)
                {
                    question.RemoveAnswerOptionById(option.Id);
                }
            }

            // Update the repository
            questionRepository.Update(question);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Question updated successfully with ID: {QuestionId}", request.Id);
            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating question with ID: {QuestionId}", request.Id);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(request.Text), "Error al actualizar la pregunta."));
        }
    }
}