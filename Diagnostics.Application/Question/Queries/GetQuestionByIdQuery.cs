using FluentValidation;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Diagnostics.Application.Question.Queries;

/// <summary>
/// Query to get a question by ID.
/// </summary>
public sealed record GetQuestionByIdQuery(long Id) : IBaseRequest<QuestionDetailDto>;

/// <summary>
/// DTO for question details.
/// </summary>
public sealed record QuestionDetailDto(
    long Id,
    string Text,
    AnswerType AnswerType,
    QuestionPhase AppliesToPhase,
    bool IsUsedForMentoringPlan,
    bool IsUsedForDiagnosis,
    List<AnswerOptionDetailDto> AnswerOptions);

/// <summary>
/// DTO for answer option details.
/// </summary>
public sealed record AnswerOptionDetailDto(
    long Id,
    string Text,
    int Score,
    FodaType Foda,
    string FodaExplanation,
    OdsrType Odsr,
    string OdsrExplanation,
    string? FollowupQuestionText,
    int Order);

/// <summary>
/// Validator for GetQuestionByIdQuery.
/// </summary>
public sealed class GetQuestionByIdQueryValidator : AbstractValidator<GetQuestionByIdQuery>
{
    public GetQuestionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la pregunta debe ser válido.");
    }
}

/// <summary>
/// Handler for GetQuestionByIdQuery.
/// </summary>
public sealed class GetQuestionByIdQueryHandler(
    IQuestionRepository questionRepository,
    ILogger<GetQuestionByIdQueryHandler> logger)
    : BaseCommandHandler<GetQuestionByIdQuery, QuestionDetailDto>
{
    public override async Task<Result<QuestionDetailDto>> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting question with ID: {QuestionId}", request.Id);

            // Get the question with answer options
            var question = await questionRepository.GetByIdWithAnswerOptionsAsync(request.Id, cancellationToken);
            if (question is null)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(request.Id), "La pregunta no existe."));
            }

            // Map to DTO
            var dto = new QuestionDetailDto(
                Id: question.Id,
                Text: question.Text,
                AnswerType: question.AnswerType,
                AppliesToPhase: question.AppliesToPhase,
                IsUsedForMentoringPlan: question.IsUsedForMentoringPlan,
                IsUsedForDiagnosis: question.IsUsedForDiagnosis,
                AnswerOptions: question.AnswerOptions
                    .OrderBy(o => o.Order)
                    .Select(o => new AnswerOptionDetailDto(
                        Id: o.Id,
                        Text: o.Text,
                        Score: o.Score,
                        Foda: o.Foda,
                        FodaExplanation: o.FodaExplanation,
                        Odsr: o.Odsr,
                        OdsrExplanation: o.OdsrExplanation,
                        FollowupQuestionText: o.FollowUpQuestionText,
                        Order: o.Order))
                    .ToList());

            logger.LogInformation("Question retrieved successfully with ID: {QuestionId}", request.Id);
            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting question with ID: {QuestionId}", request.Id);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(request.Id), "Error al obtener la pregunta."));
        }
    }
}