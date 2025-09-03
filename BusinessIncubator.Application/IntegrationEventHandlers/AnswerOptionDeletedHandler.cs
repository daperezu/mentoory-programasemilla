using MediatR;
using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Diagnostics.Application.IntegrationEvents;
using LinaSys.Shared.Domain.SeedWork;
namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the AnswerOptionDeleted integration event to clear source references in project answer options.
/// </summary>
public sealed class AnswerOptionDeletedHandler(
    IBusinessIncubatorRepository repository,
    ILogger<AnswerOptionDeletedHandler> logger) : INotificationHandler<AnswerOptionDeleted>
{
    public async Task Handle(AnswerOptionDeleted notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing answer option deletion sync for AnswerOptionId: {AnswerOptionId}",
            notification.OptionId);

        try
        {
            // Get all project answer options that reference this source answer option
            var projectAnswerOptionReferences = await repository
                .GetProjectAnswerOptionReferencesBySourceIdAsync(notification.OptionId, cancellationToken);

            if (projectAnswerOptionReferences.Count == 0)
            {
                logger.LogInformation(
                    "No project answer options found for source AnswerOptionId: {AnswerOptionId}",
                    notification.OptionId);
                return;
            }

            foreach (var reference in projectAnswerOptionReferences)
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

                // Find the answer option in the project blocks
                var answerOption = project.ProjectBlocks
                    .SelectMany(b => b.ProjectQuestions)
                    .SelectMany(q => q.ProjectAnswerOptions)
                    .FirstOrDefault(ao => ao.Id == reference.AnswerOptionId);

                if (answerOption is null)
                {
                    logger.LogWarning(
                        "Answer option {AnswerOptionId} not found in project {ProjectId}",
                        reference.AnswerOptionId,
                        reference.ProjectId);
                    continue;
                }

                // Clear the source reference
                answerOption.ClearSource();
                logger.LogInformation(
                    "Cleared source reference for answer option {AnswerOptionId} in project {ProjectId}",
                    reference.AnswerOptionId,
                    reference.ProjectId);

                // Update the project
                repository.Update(businessIncubator);
            }

            // Save all changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully cleared source references for {Count} project answer options for deleted AnswerOptionId: {AnswerOptionId}",
                projectAnswerOptionReferences.Count,
                notification.OptionId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error clearing source references for deleted AnswerOptionId: {AnswerOptionId}",
                notification.OptionId);
            throw;
        }
    }
}
