using MediatR;
using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.KnowledgeStructure.Application.IntegrationEvents;
using LinaSys.Shared.Domain.SeedWork;
namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the TopicUpdated integration event to synchronize project topics.
/// </summary>
public sealed class TopicUpdatedHandler(
    IBusinessIncubatorRepository repository,
    ILogger<TopicUpdatedHandler> logger) : INotificationHandler<TopicUpdated>
{
    public async Task Handle(TopicUpdated notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing topic update sync for TopicId: {TopicId}",
            notification.TopicId);

        try
        {
            // Get all project topics that reference this source topic
            var projectTopicReferences = await repository
                .GetProjectTopicReferencesBySourceIdAsync(notification.TopicId, cancellationToken);

            if (projectTopicReferences.Count == 0)
            {
                logger.LogInformation(
                    "No project topics found for source TopicId: {TopicId}",
                    notification.TopicId);
                return;
            }

            foreach (var reference in projectTopicReferences)
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

                // Get the project knowledge structure
                var knowledgeStructure = await repository
                    .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

                if (knowledgeStructure is null)
                {
                    logger.LogWarning(
                        "Knowledge structure not found for project {ProjectId}",
                        reference.ProjectId);
                    continue;
                }

                // Find the topic in the knowledge structure
                var topic = knowledgeStructure.ProjectModules
                    .SelectMany(m => m.ProjectTopics)
                    .FirstOrDefault(t => t.Id == reference.TopicId);

                if (topic is null)
                {
                    logger.LogWarning(
                        "Topic {TopicId} not found in project {ProjectId}",
                        reference.TopicId,
                        reference.ProjectId);
                    continue;
                }

                // Update only non-customized fields
                if (!topic.IsNameCustomized && topic.Name != notification.Name)
                {
                    topic.UpdateName(notification.Name, isCustomized: false);
                    logger.LogInformation(
                        "Updated name for topic {TopicId} in project {ProjectId}",
                        reference.TopicId,
                        reference.ProjectId);
                }

                if (!topic.IsOrderCustomized && topic.Order != notification.Order)
                {
                    topic.UpdateOrder(notification.Order, isOrderCustomized: false);
                    logger.LogInformation(
                        "Updated order for topic {TopicId} in project {ProjectId}",
                        reference.TopicId,
                        reference.ProjectId);
                }

                // Update the business incubator
                repository.Update(businessIncubator);
            }

            // Save all changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully synchronized {Count} project topics for source TopicId: {TopicId}",
                projectTopicReferences.Count,
                notification.TopicId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error synchronizing project topics for source TopicId: {TopicId}",
                notification.TopicId);
            throw;
        }
    }
}
