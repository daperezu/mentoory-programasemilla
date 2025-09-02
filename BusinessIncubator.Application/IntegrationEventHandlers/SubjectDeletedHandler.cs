using MediatR;
using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.KnowledgeStructure.Application.IntegrationEvents;
using LinaSys.Shared.Domain.SeedWork;
namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the SubjectDeleted integration event to clear source references in project subjects.
/// </summary>
public sealed class SubjectDeletedHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SubjectDeletedHandler> logger) : INotificationHandler<SubjectDeleted>
{
    public async Task Handle(SubjectDeleted notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing subject deletion sync for SubjectId: {SubjectId}",
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
                        "Project {ProjectId} or its knowledge structure not found",
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

                // Clear the source reference
                subject.ClearSource();
                logger.LogInformation(
                    "Cleared source reference for subject {SubjectId} in project {ProjectId}",
                    reference.SubjectId,
                    reference.ProjectId);

                // Update the project
                repository.Update(businessIncubator);
            }

            // Save all changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully cleared source references for {Count} project subjects for deleted SubjectId: {SubjectId}",
                projectSubjectReferences.Count,
                notification.SubjectId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error clearing source references for deleted SubjectId: {SubjectId}",
                notification.SubjectId);
            throw;
        }
    }
}
