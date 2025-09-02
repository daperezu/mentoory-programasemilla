using FluentValidation;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Orchestration.Application.Diagnostics.Queries;

public sealed record GetProjectFormByExternalIdForDiagnosisQuery(Guid ExternalId, int DiagnosisPhase) : IBaseRequest<List<BlockDto>>;

public sealed record BlockDto(long Id, string Title, List<QuestionDto> Questions);

public sealed record QuestionDto(long Id, int AnswerType, string Text, bool Overriden, List<AnswerOptionDto> Options);

public sealed record AnswerOptionDto(long Id, string Text, string? FollowUpQuestionText, bool Overriden);

public sealed class GetProjectFormByExternalIdForDiagnosisQueryValidator : AbstractValidator<GetProjectFormByExternalIdForDiagnosisQuery>
{
    public GetProjectFormByExternalIdForDiagnosisQueryValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty()
            .WithMessage("External ID cannot be empty.");

        RuleFor(x => x.DiagnosisPhase)
            .Must(value => Enum.IsDefined(typeof(QuestionPhase), value))
            .WithMessage("DiagnosisPhase must be a valid enum value.");
    }
}

public sealed class GetProjectFormByExternalIdForDiagnosisQueryHandler(
    LinaSys.BusinessIncubator.Domain.Repositories.IBusinessIncubatorRepository businessIncubatorRepository)
    : BaseCommandHandler<GetProjectFormByExternalIdForDiagnosisQuery, List<BlockDto>>
{
    public override async Task<Result<List<BlockDto>>> Handle(GetProjectFormByExternalIdForDiagnosisQuery request, CancellationToken cancellationToken)
    {
        // Use the specialized repository method that respects DDD boundaries
        var diagnosisBlocks = await businessIncubatorRepository.GetProjectDiagnosisQuestionsAsync(
            request.ExternalId,
            request.DiagnosisPhase,
            cancellationToken).ConfigureAwait(false);

        if (!diagnosisBlocks.Any())
        {
            // Check if project exists
            var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(request.ExternalId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ExternalId), "Project not found."));
            }

            // Project exists but has no questions for this phase
            return Success([]);
        }

        // Map from repository DTOs to query DTOs
        var blocks = diagnosisBlocks.Select(block => new BlockDto(
            block.Id,
            block.Title,
            block.Questions.Select(question => new QuestionDto(
                question.Id,
                question.AnswerType,
                question.Text,
                question.IsTextCustomized,
                question.Options.Select(option => new AnswerOptionDto(
                    option.Id,
                    option.Text,
                    option.FollowUpQuestionText,
                    option.IsTextCustomized))
                .ToList()))
            .ToList()))
        .ToList();

        return Success(blocks);
    }
}
