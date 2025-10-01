using MediatR;
using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Diagnostics.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the QuestionDeleted integration event to clear source references in project questions.
/// </summary>
public sealed class QuestionDeletedHandler(
    IBusinessIncubatorRepository repository,
    ILogger<QuestionDeletedHandler> logger) : INotificationHandler<QuestionDeleted>
{
    public async Task Handle(QuestionDeleted notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing question deletion sync for QuestionId: {QuestionId}",
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

                // Clear the source reference
                question.ClearSource();
                logger.LogInformation(
                    "Cleared source reference for question {QuestionId} in project {ProjectId}",
                    reference.QuestionId,
                    reference.ProjectId);

                // Update the project
                repository.Update(businessIncubator);
            }

            // Save all changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully cleared source references for {Count} project questions for deleted QuestionId: {QuestionId}",
                projectQuestionReferences.Count,
                notification.QuestionId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error clearing source references for deleted QuestionId: {QuestionId}",
                notification.QuestionId);
            throw;
        }
    }
}
