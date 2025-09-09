using System.Text.Json;
using LinaSys.BusinessIncubator.Application.IntegrationEvents;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.Diagnostics.Domain.Aggregates.UserProjectDiagnosis;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;
using BusinessIncubatorEnums = LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Orchestration.Application.BusinessIncubator.EventHandlers;

/// <summary>
/// Handles the ProjectFormSubmissionApproved event to create DiagnosisAnswers.
/// </summary>
public sealed class ProjectFormSubmissionApprovedHandler(
    IUserProjectDiagnosisRepository repository,
    ILogger<ProjectFormSubmissionApprovedHandler> logger,
    ITimeProvider timeProvider) : INotificationHandler<ProjectFormSubmissionApproved>
{
    public async Task Handle(ProjectFormSubmissionApproved notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing approved form submission for project {ProjectId}, participant {ParticipantUserId}",
                notification.ProjectId,
                notification.ParticipantUserId);

            // Parse draft data
            if (string.IsNullOrWhiteSpace(notification.DraftData))
            {
                logger.LogWarning("Approved form submission has no draft data");
                return;
            }

            var draftData = JsonSerializer.Deserialize<DraftDataDto>(notification.DraftData);
            if (draftData is null)
            {
                logger.LogWarning("Failed to deserialize draft data");
                return;
            }

            // Get or create user project diagnosis
            var diagnosis = await repository.GetByProjectAndUserAsync(
                notification.ProjectId,
                notification.ParticipantUserId,
                cancellationToken)
                ?? UserProjectDiagnosis.Create(
                    notification.ProjectId,
                    notification.ParticipantUserId,
                    timeProvider.UtcNow);

            // TODO: Get the actual phase from the form or project
            // For now, defaulting to Start phase
            var phase = QuestionPhase.Start;

            // TODO: Get question metadata from BusinessIncubator repository
            // This would include FODA, ODSR, scores, etc.
            var answerInputList = new List<DiagnosisAnswerInput>();

            // Process each block
            foreach (var blockResponse in draftData.BlockResponses)
            {
                // Process each question in the block
                foreach (var questionResponse in blockResponse.QuestionResponses.Where(q => q.IsAnswered))
                {
                    // Parse answer based on type
                    if (string.IsNullOrWhiteSpace(questionResponse.Answer))
                    {
                        continue;
                    }

                    // Handle different answer types
                    if (questionResponse.AnswerType == (int)BusinessIncubatorEnums.AnswerType.MultiChoice)
                    {
                        // For multi-choice, parse comma-separated option IDs
                        var optionIds = questionResponse.Answer.Split(',')
                            .Select(id => id.Trim())
                            .Where(id => long.TryParse(id, out _))
                            .Select(long.Parse)
                            .ToList();

                        foreach (var answerOptionId in optionIds)
                        {
                            var answerInput = CreateDiagnosisAnswerInput(
                                notification,
                                blockResponse,
                                questionResponse,
                                answerOptionId);

                            answerInputList.Add(answerInput);
                        }
                    }
                    else if (questionResponse.AnswerType == (int)BusinessIncubatorEnums.AnswerType.SingleChoice)
                    {
                        // For single choice questions
                        if (long.TryParse(questionResponse.Answer, out var optionId))
                        {
                            var answerInput = CreateDiagnosisAnswerInput(
                                notification,
                                blockResponse,
                                questionResponse,
                                optionId);

                            answerInputList.Add(answerInput);
                        }
                    }
                    else
                    {
                        // For text/numeric/date questions without answer options
                        var answerInput = CreateDiagnosisAnswerInputForFreeForm(
                            notification,
                            blockResponse,
                            questionResponse);

                        answerInputList.Add(answerInput);
                    }
                }
            }

            // Add answers through aggregate
            if (answerInputList.Any())
            {
                diagnosis.AddOrUpdateAnswersFromApprovedSubmission(
                    phase,
                    answerInputList,
                    notification.ApprovedAt,
                    timeProvider.UtcNow);

                // Save through repository
                if (diagnosis.Id == 0)
                {
                    repository.Add(diagnosis);
                }
                else
                {
                    repository.Update(diagnosis);
                }

                await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Created/updated diagnosis with {Count} answers for project {ProjectId}, participant {ParticipantUserId}",
                    answerInputList.Count,
                    notification.ProjectId,
                    notification.ParticipantUserId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing approved form submission for project {ProjectId}",
                notification.ProjectId);
            // Don't throw - we don't want to fail the approval process
        }
    }

    private DiagnosisAnswerInput CreateDiagnosisAnswerInput(
        ProjectFormSubmissionApproved notification,
        BlockResponseDto blockResponse,
        QuestionResponseDto questionResponse,
        long answerOptionId)
    {
        // TODO: We need to fetch actual answer option details from the database
        // For now, creating with minimal information
        return new DiagnosisAnswerInput
        {
            ProjectId = notification.ProjectId,
            UserId = notification.ParticipantUserId,
            TopicId = questionResponse.TopicInfo?.TopicId,
            TopicName = questionResponse.TopicInfo?.TopicName ?? string.Empty,
            ModuleId = questionResponse.ModuleInfo?.ModuleId,
            ModuleName = questionResponse.ModuleInfo?.ModuleName ?? string.Empty,
            BlockId = blockResponse.BlockId,
            BlockName = blockResponse.BlockName,
            QuestionId = questionResponse.QuestionId,
            QuestionText = questionResponse.QuestionText,
            AnswerOptionId = answerOptionId,
            AnswerOptionText = string.Empty, // TODO: Fetch from database
            AnswerOptionUserInput = questionResponse.Answer ?? string.Empty,
            FollowUpQuestionText = string.Empty, // TODO: Fetch if exists
            FollowUpAnswerUserInput = questionResponse.FollowUpAnswer ?? string.Empty,
            Score = 0, // TODO: Calculate based on answer option
            Foda = FodaType.NoDefinido,
            FodaExplanation = string.Empty,
            Odsr = OdsrType.NoDefinido,
            OdsrExplanation = string.Empty,
            IsUsedForMentoringPlan = false, // TODO: Get from question
            IsUsedForDiagnosis = true // TODO: Get from question
        };
    }

    private DiagnosisAnswerInput CreateDiagnosisAnswerInputForFreeForm(
        ProjectFormSubmissionApproved notification,
        BlockResponseDto blockResponse,
        QuestionResponseDto questionResponse)
    {
        // For free-form answers, we use a special answer option ID (0 or negative)
        var answerText = questionResponse.Answer ?? string.Empty;

        return new DiagnosisAnswerInput
        {
            ProjectId = notification.ProjectId,
            UserId = notification.ParticipantUserId,
            TopicId = questionResponse.TopicInfo?.TopicId,
            TopicName = questionResponse.TopicInfo?.TopicName ?? string.Empty,
            ModuleId = questionResponse.ModuleInfo?.ModuleId,
            ModuleName = questionResponse.ModuleInfo?.ModuleName ?? string.Empty,
            BlockId = blockResponse.BlockId,
            BlockName = blockResponse.BlockName,
            QuestionId = questionResponse.QuestionId,
            QuestionText = questionResponse.QuestionText,
            AnswerOptionId = 0, // Special ID for free-form answers
            AnswerOptionText = "Respuesta libre",
            AnswerOptionUserInput = answerText,
            FollowUpQuestionText = string.Empty,
            FollowUpAnswerUserInput = questionResponse.FollowUpAnswer ?? string.Empty,
            Score = 0,
            Foda = FodaType.NoDefinido,
            FodaExplanation = string.Empty,
            Odsr = OdsrType.NoDefinido,
            OdsrExplanation = string.Empty,
            IsUsedForMentoringPlan = false, // TODO: Get from question
            IsUsedForDiagnosis = true // TODO: Get from question
        };
    }
}
