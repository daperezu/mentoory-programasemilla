using System.Text.Json;
using LinaSys.BusinessIncubator.Application.IntegrationEvents;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.BusinessIncubator.Domain.Repositories;
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
    IUserProjectDiagnosisRepository diagnosisRepository,
    IBusinessIncubatorRepository businessIncubatorRepository,
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
            var diagnosis = await diagnosisRepository.GetByProjectAndUserAsync(
                notification.ProjectId,
                notification.ParticipantUserId,
                cancellationToken)
                ?? UserProjectDiagnosis.Create(
                    notification.ProjectId,
                    notification.ParticipantUserId,
                    timeProvider.UtcNow);

            // Use phase from the event
            var phase = ConvertFromBusinessIncubatorPhase(notification.Phase);

            // Get question metadata from BusinessIncubator repository
            var questionMetadata = await businessIncubatorRepository.GetProjectQuestionsWithAnswerOptionsAsync(
                notification.ProjectId,
                notification.Phase,
                cancellationToken);

            // Collect all answer option IDs to fetch metadata
            var allAnswerOptionIds = new List<long>();
            foreach (var blockResponse in draftData.BlockResponses)
            {
                foreach (var questionResponse in blockResponse.QuestionResponses.Where(q => q.IsAnswered))
                {
                    if (questionResponse.AnswerType == (int)BusinessIncubatorEnums.AnswerType.MultiChoice ||
                        questionResponse.AnswerType == (int)BusinessIncubatorEnums.AnswerType.SingleChoice)
                    {
                        var optionIds = ExtractAnswerOptionIds(questionResponse.Answer);
                        allAnswerOptionIds.AddRange(optionIds);
                    }
                }
            }

            // Get answer option metadata
            var answerOptions = await businessIncubatorRepository.GetAnswerOptionsByIdsAsync(
                allAnswerOptionIds.Distinct().ToList(),
                cancellationToken);
            var answerOptionMap = answerOptions.ToDictionary(ao => ao.Id);

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
                        var optionIds = ExtractAnswerOptionIds(questionResponse.Answer);

                        foreach (var answerOptionId in optionIds)
                        {
                            var answerInput = CreateDiagnosisAnswerInput(
                                notification,
                                blockResponse,
                                questionResponse,
                                answerOptionId,
                                questionMetadata,
                                answerOptionMap);

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
                                optionId,
                                questionMetadata,
                                answerOptionMap);

                            answerInputList.Add(answerInput);
                        }
                    }
                    else
                    {
                        // For text/numeric/date questions without answer options
                        var answerInput = CreateDiagnosisAnswerInputForFreeForm(
                            notification,
                            blockResponse,
                            questionResponse,
                            questionMetadata);

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
                    diagnosisRepository.Add(diagnosis);
                }
                else
                {
                    diagnosisRepository.Update(diagnosis);
                }

                await diagnosisRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

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
        long answerOptionId,
        Dictionary<long, LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectQuestion> questionMetadata,
        Dictionary<long, LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectAnswerOption> answerOptionMap)
    {
        // Get question metadata
        var question = questionMetadata.ContainsKey(questionResponse.QuestionId)
            ? questionMetadata[questionResponse.QuestionId]
            : null;
        // Get answer option metadata
        var answerOption = answerOptionMap.ContainsKey(answerOptionId)
            ? answerOptionMap[answerOptionId]
            : null;
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
            QuestionText = question?.Text ?? questionResponse.QuestionText,
            AnswerOptionId = answerOptionId,
            AnswerOptionText = answerOption?.Text ?? string.Empty,
            AnswerOptionUserInput = questionResponse.Answer ?? string.Empty,
            FollowUpQuestionText = answerOption?.FollowUpQuestionText ?? string.Empty,
            FollowUpAnswerUserInput = questionResponse.FollowUpAnswer ?? string.Empty,
            Score = answerOption?.Score ?? 0,
            Foda = ConvertToFodaType(answerOption?.Foda),
            FodaExplanation = answerOption?.FodaExplanation ?? string.Empty,
            Odsr = ConvertToOdsrType(answerOption?.Odsr),
            OdsrExplanation = answerOption?.OdsrExplanation ?? string.Empty,
            IsUsedForMentoringPlan = question?.IsUsedForMentoringPlan ?? false,
            IsUsedForDiagnosis = question?.IsUsedForDiagnosis ?? true
        };
    }

    private DiagnosisAnswerInput CreateDiagnosisAnswerInputForFreeForm(
        ProjectFormSubmissionApproved notification,
        BlockResponseDto blockResponse,
        QuestionResponseDto questionResponse,
        Dictionary<long, LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectQuestion> questionMetadata)
    {
        // Get question metadata
        var question = questionMetadata.ContainsKey(questionResponse.QuestionId)
            ? questionMetadata[questionResponse.QuestionId]
            : null;
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
            QuestionText = question?.Text ?? questionResponse.QuestionText,
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
            IsUsedForMentoringPlan = question?.IsUsedForMentoringPlan ?? false,
            IsUsedForDiagnosis = question?.IsUsedForDiagnosis ?? true
        };
    }

    private QuestionPhase ConvertFromBusinessIncubatorPhase(BusinessIncubatorEnums.QuestionPhase phase)
    {
        return phase switch
        {
            BusinessIncubatorEnums.QuestionPhase.Start => QuestionPhase.Start,
            BusinessIncubatorEnums.QuestionPhase.Final => QuestionPhase.Final,
            _ => QuestionPhase.Start // Default to Start for undefined
        };
    }

    private BusinessIncubatorEnums.QuestionPhase ConvertToBusinessIncubatorPhase(QuestionPhase phase)
    {
        return phase switch
        {
            QuestionPhase.Start => BusinessIncubatorEnums.QuestionPhase.Start,
            QuestionPhase.Final => BusinessIncubatorEnums.QuestionPhase.Final,
            _ => BusinessIncubatorEnums.QuestionPhase.Undefined
        };
    }

    private List<long> ExtractAnswerOptionIds(string? answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            return new List<long>();
        }

        return answer.Split(',')
            .Select(id => id.Trim())
            .Where(id => long.TryParse(id, out _))
            .Select(long.Parse)
            .ToList();
    }

    private FodaType ConvertToFodaType(BusinessIncubatorEnums.FodaType? fodaType)
    {
        if (!fodaType.HasValue)
        {
            return FodaType.NoDefinido;
        }

        // Both enums use the same char values, convert by matching the underlying value
        return fodaType.Value switch
        {
            BusinessIncubatorEnums.FodaType.NoDefinido => FodaType.NoDefinido,
            BusinessIncubatorEnums.FodaType.Fortalezas => FodaType.Fortalezas,
            BusinessIncubatorEnums.FodaType.Oportunidades => FodaType.Oportunidades,
            BusinessIncubatorEnums.FodaType.Debilidades => FodaType.Debilidades,
            BusinessIncubatorEnums.FodaType.Amenazas => FodaType.Amenazas,
            _ => FodaType.NoDefinido
        };
    }

    private OdsrType ConvertToOdsrType(BusinessIncubatorEnums.OdsrType? odsrType)
    {
        if (!odsrType.HasValue)
        {
            return OdsrType.NoDefinido;
        }

        // Both enums use the same char values, convert by matching the underlying value
        return odsrType.Value switch
        {
            BusinessIncubatorEnums.OdsrType.NoDefinido => OdsrType.NoDefinido,
            BusinessIncubatorEnums.OdsrType.Ofensiva => OdsrType.Ofensiva,
            BusinessIncubatorEnums.OdsrType.Defensiva => OdsrType.Defensiva,
            BusinessIncubatorEnums.OdsrType.Supervivencia => OdsrType.Supervivencia,
            BusinessIncubatorEnums.OdsrType.Reorientacion => OdsrType.Reorientacion,
            _ => OdsrType.NoDefinido
        };
    }
}
