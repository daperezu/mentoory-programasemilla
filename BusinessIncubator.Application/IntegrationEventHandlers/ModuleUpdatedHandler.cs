using MediatR;
using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.KnowledgeStructure.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the ModuleUpdated integration event to synchronize project modules.
/// </summary>
public sealed class ModuleUpdatedHandler(
    IBusinessIncubatorRepository repository,
    ILogger<ModuleUpdatedHandler> logger) : INotificationHandler<ModuleUpdated>
{
    public async Task Handle(ModuleUpdated notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing module update sync for ModuleId: {ModuleId}",
            notification.ModuleId);

        try
        {
            // Get all project modules that reference this source module
            var projectModuleReferences = await repository
                .GetProjectModuleReferencesBySourceIdAsync(notification.ModuleId, cancellationToken);

            if (projectModuleReferences.Count == 0)
            {
                logger.LogInformation(
                    "No project modules found for source ModuleId: {ModuleId}",
                    notification.ModuleId);
                return;
            }

            foreach (var reference in projectModuleReferences)
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

                // Find the module in the knowledge structure
                var module = knowledgeStructure.ProjectModules
                    .FirstOrDefault(m => m.Id == reference.ModuleId);

                if (module is null)
                {
                    logger.LogWarning(
                        "Module {ModuleId} not found in project {ProjectId}",
                        reference.ModuleId,
                        reference.ProjectId);
                    continue;
                }

                // Update only non-customized fields
                if (!module.IsNameCustomized && module.Name != notification.Name)
                {
                    module.UpdateName(notification.Name, isNameCustomized: false);
                    logger.LogInformation(
                        "Updated name for module {ModuleId} in project {ProjectId}",
                        reference.ModuleId,
                        reference.ProjectId);
                }

                if (!module.IsOrderCustomized && module.Order != notification.Order)
                {
                    module.UpdateOrder(notification.Order, isOrderCustomized: false);
                    logger.LogInformation(
                        "Updated order for module {ModuleId} in project {ProjectId}",
                        reference.ModuleId,
                        reference.ProjectId);
                }

                // Update the business incubator
                repository.Update(businessIncubator);
            }

            // Save all changes
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully synchronized {Count} project modules for source ModuleId: {ModuleId}",
                projectModuleReferences.Count,
                notification.ModuleId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error synchronizing project modules for source ModuleId: {ModuleId}",
                notification.ModuleId);
            throw;
        }
    }
}
