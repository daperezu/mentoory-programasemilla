using MediatR;
using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Diagnostics.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the QuestionUpdated integration event to synchronize project questions.
/// </summary>
public sealed class QuestionUpdatedHandler(
    IBusinessIncubatorRepository repository,
    ILogger<QuestionUpdatedHandler> logger) : INotificationHandler<QuestionUpdated>
{
    public async Task Handle(QuestionUpdated notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing question update sync for QuestionId: {QuestionId}",
            notification.QuestionId);

        try
        {
            // Get all project questions that reference this source question
            var projectQuestionReferences = await repository
                .GetProjectQuestionReferencesBySourceIdAsync(notification.QuestionId, cancellationToken);

            if (projectQuestionReferences.Count == 0)
            {
                logger.LogInformation(
                    "No project questions found for source QuestionId: {QuestionId}",
                    notification.QuestionId);
                return;
            }

            foreach (var reference in projectQuestionReferences)
            {
                // Load the business incubator by project ID
                var businessIncubator = await repository
                    .GetByProjectIdAsync(reference.ProjectId, cancellationToken);

                if (businessIncubator is null)
                {
                    logger.LogWarning(
                        "Business incubator not found for project {ProjectId}",
                        reference.ProjectId);
                    continue;
                }

                // Get the project directly
                var project = await repository
                    .GetProjectByIdAsync(reference.ProjectId, cancellationToken);

                if (project is null)
                {
                    logger.LogWarning(
                        "Project {ProjectId} not found",
                        reference.ProjectId);
                    continue;
                }

                // Get the project with blocks
                project = await repository
                    .GetProjectWithBlocksByIdAsync(project.Id, cancellationToken);

                if (project is null)
                {
                    logger.LogWarning(
                        "Project {ProjectId} not found with blocks",
                        reference.ProjectId);
                    continue;
                }

                // Find the question in the project blocks
                var question = project.ProjectBlocks
                    .SelectMany(b => b.ProjectQuestions)
                    .FirstOrDefault(q => q.Id == reference.QuestionId);

                if (question is null)
                {
                    logger.LogWarning(
                        "Question {QuestionId} not found in project {ProjectId}",
                        reference.QuestionId,
                        reference.ProjectId);
                    continue;
                }

                // Update only non-customized fields
                if (!question.IsTextCustomized && question.Text != notification.Text)
                {
                    question.UpdateText(notification.Text, isCustomized: false);
                    logger.LogInformation(
                        "Updated text for question {QuestionId} in project {ProjectId}",
                        reference.QuestionId,
                        reference.ProjectId);
                }

                if (!question.IsAnswerTypeCustomized && (int)question.AnswerType != notification.AnswerType)
                {
                    question.UpdateAnswerType((AnswerType)notification.AnswerType, isCustomized: false);
                    logger.LogInformation(
                        "Updated answer type for question {QuestionId} in project {ProjectId}",
                        reference.QuestionId,
                        reference.ProjectId);
                }

                if (!question.IsAppliesToPhaseCustomized && (int)question.AppliesToPhase != notification.AppliesToPhase)
                {
                    question.UpdateAppliesToPhase((QuestionPhase)notification.AppliesToPhase, isCustomized: false);
                    logger.LogInformation(
                        "Updated applies to phase for question {QuestionId} in project {ProjectId}",
                        reference.QuestionId,
                        reference.ProjectId);
                }

                if (!question.IsMentoringPlanCustomized && question.IsUsedForMentoringPlan != notification.IsUsedForMentoringPlan)
                {
                    question.UpdateIsUsedForMentoringPlan(notification.IsUsedForMentoringPlan, isMentoringPlanCustomized: false);
                    logger.LogInformation(
                        "Updated mentoring plan usage for question {QuestionId} in project {ProjectId}",
                        reference.QuestionId,
                        reference.ProjectId);
                }

                if (!question.IsDiagnosisCustomized && question.IsUsedForDiagnosis != notification.IsUsedForDiagnosis)
                {
                    question.UpdateIsUsedForDiagnosis(notification.IsUsedForDiagnosis, isDiagnosisCustomized: false);
                    logger.LogInformation(
                        "Updated diagnosis usage for question {QuestionId} in project {ProjectId}",
                        reference.QuestionId,
                        reference.ProjectId);
                }

                if (!question.IsOrderCustomized && question.Order != notification.Order)
                {
                    question.UpdateOrder(notification.Order, isCustomized: false);
                    logger.LogInformation(
                        "Updated order for question {QuestionId} in project {ProjectId}",
                        reference.QuestionId,
                        reference.ProjectId);
                }

                // Update the project
                repository.Update(businessIncubator);
            }

            // Save all changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully synchronized {Count} project questions for source QuestionId: {QuestionId}",
                projectQuestionReferences.Count,
                notification.QuestionId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error synchronizing project questions for source QuestionId: {QuestionId}",
                notification.QuestionId);
            throw;
        }
    }
}
