using LinaSys.BusinessIncubator.Application.IntegrationEvents;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.BusinessIncubator.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the DiagnosticsModuleDeletedIntegrationEvent to orphan project modules.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DiagnosticsModuleDeletedEventHandler"/> class.
/// </remarks>
public class DiagnosticsModuleDeletedEventHandler(
    IBusinessIncubatorRepository repository,
    BusinessIncubatorDbContext dbContext,
    ILogger<DiagnosticsModuleDeletedEventHandler> logger) : INotificationHandler<DiagnosticsModuleDeletedIntegrationEvent>
{

    /// <inheritdoc/>
    public async Task Handle(
        DiagnosticsModuleDeletedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing module deletion for ModuleId: {ModuleId}",
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
                "Found {Count} projects to orphan for ModuleId: {ModuleId}",
                projects.Count,
                notification.ModuleId);

            // Process each project
            foreach (var project in projects)
            {
                var knowledgeStructure = project.GetKnowledgeStructure();

                if (knowledgeStructure is null)
                {
                    continue;
                }

                var module = knowledgeStructure.FindModuleBySourceId(notification.ModuleId);
                if (module is not null)
                {
                    // Clear the source reference (orphan the module)
                    module.ClearSourceReference();

                    logger.LogInformation(
                        "Orphaned module in Project: {ProjectId}, Module: {ModuleId}",
                        project.Id,
                        module.Id);

                    // Mark project as modified
                    repository.Update(project);
                }
            }

            // Save all changes
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Successfully orphaned all project modules for deleted ModuleId: {ModuleId}",
                notification.ModuleId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing module deletion for ModuleId: {ModuleId}",
                notification.ModuleId);
            throw;
        }
    }
}