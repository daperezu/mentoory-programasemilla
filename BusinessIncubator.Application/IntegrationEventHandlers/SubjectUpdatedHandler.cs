using MediatR;
using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.KnowledgeStructure.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the SubjectUpdated integration event to synchronize project subjects.
/// </summary>
public sealed class SubjectUpdatedHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SubjectUpdatedHandler> logger) : INotificationHandler<SubjectUpdated>
{
    public async Task Handle(SubjectUpdated notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing subject update sync for SubjectId: {SubjectId}",
            notification.SubjectId);

        try
        {
            // Get all project subjects that reference this source subject
            var projectSubjectReferences = await repository
                .GetProjectSubjectReferencesBySourceIdAsync(notification.SubjectId, cancellationToken);

            if (projectSubjectReferences.Count == 0)
            {
                logger.LogInformation(
                    "No project subjects found for source SubjectId: {SubjectId}",
                    notification.SubjectId);
                return;
            }

            foreach (var reference in projectSubjectReferences)
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

                // Find the subject in the knowledge structure
                var subject = knowledgeStructure.ProjectModules
                    .SelectMany(m => m.ProjectTopics)
                    .SelectMany(t => t.ProjectSubjects)
                    .FirstOrDefault(s => s.Id == reference.SubjectId);

                if (subject is null)
                {
                    logger.LogWarning(
                        "Subject {SubjectId} not found in project {ProjectId}",
                        reference.SubjectId,
                        reference.ProjectId);
                    continue;
                }

                // Update only non-customized fields
                if (!subject.IsTitleCustomized && subject.Title != notification.Title)
                {
                    subject.UpdateTitle(notification.Title, isCustomized: false);
                    logger.LogInformation(
                        "Updated title for subject {SubjectId} in project {ProjectId}",
                        reference.SubjectId,
                        reference.ProjectId);
                }

                if (!subject.IsContentCustomized && subject.Content != notification.Content)
                {
                    subject.UpdateContent(notification.Content ?? string.Empty, isCustomized: false);
                    logger.LogInformation(
                        "Updated content for subject {SubjectId} in project {ProjectId}",
                        reference.SubjectId,
                        reference.ProjectId);
                }

                if (!subject.IsOrderCustomized && subject.Order != notification.Order)
                {
                    subject.UpdateOrder(notification.Order, isCustomized: false);
                    logger.LogInformation(
                        "Updated order for subject {SubjectId} in project {ProjectId}",
                        reference.SubjectId,
                        reference.ProjectId);
                }

                // Update the business incubator
                repository.Update(businessIncubator);
            }

            // Save all changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully synchronized {Count} project subjects for source SubjectId: {SubjectId}",
                projectSubjectReferences.Count,
                notification.SubjectId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error synchronizing project subjects for source SubjectId: {SubjectId}",
                notification.SubjectId);
            throw;
        }
    }
}
