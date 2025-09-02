using LinaSys.BusinessIncubator.Application.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.BusinessIncubator.Domain.ValueObjects;
using LinaSys.BusinessIncubator.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the DiagnosticsModuleUpdatedIntegrationEvent to sync project modules.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DiagnosticsModuleUpdatedEventHandler"/> class.
/// </remarks>
public class DiagnosticsModuleUpdatedEventHandler(
    IBusinessIncubatorRepository repository,
    BusinessIncubatorDbContext dbContext,
    ILogger<DiagnosticsModuleUpdatedEventHandler> logger) : INotificationHandler<DiagnosticsModuleUpdatedIntegrationEvent>
{

    /// <inheritdoc/>
    public async Task Handle(
        DiagnosticsModuleUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing module update for ModuleId: {ModuleId}",
                notification.ModuleId);

            // Find all projects using this module as source
            var projects = await repository.GetProjectsBySourceModuleAsync(
                notification.ModuleId,
                cancellationToken);

            if (!projects.Any())
            {
                logger.LogInformation(
                    "No projects found using ModuleId: {ModuleId} as source",
                    notification.ModuleId);
                return;
            }

            logger.LogInformation(
                "Found {Count} projects to sync for ModuleId: {ModuleId}",
                projects.Count,
                notification.ModuleId);

            // Create source module representation
            var sourceModule = new Module
            {
                Id = notification.ModuleId,
                Name = notification.Name,
                Order = notification.Order
            };

            var sourceModules = new Dictionary<long, Module>
            {
                { notification.ModuleId, sourceModule }
            };

            // Process each project
            foreach (var project in projects)
            {
                var knowledgeStructure = project.GetKnowledgeStructure();

                if (knowledgeStructure is null)
                {
                    continue;
                }

                var module = knowledgeStructure.FindModuleBySourceId(notification.ModuleId);
                if (module is not null && !module.IsFullyCustomized())
                {
                    // Sync the module
                    var syncResult = module.SyncFromSource(
                        sourceModules,
                        null, // No topics in this event
                        null, // No subjects in this event
                        null); // No questions in this event

                    if (syncResult.HasChanges)
                    {
                        logger.LogInformation(
                            "Applied {ChangeCount} changes to Project: {ProjectId}, Module: {ModuleId}",
                            syncResult.TotalChanges,
                            project.Id,
                            module.Id);

                        // Mark project as modified
                        repository.Update(project);
                    }
                }
            }

            // Save all changes
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully synced all projects for ModuleId: {ModuleId}",
                notification.ModuleId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing module update for ModuleId: {ModuleId}",
                notification.ModuleId);
            throw;
        }
    }
}