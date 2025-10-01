using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectModule;

/// <summary>
/// Handler for synchronizing a project module with its source.
/// </summary>
public sealed partial class SyncProjectModuleCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SyncProjectModuleCommandHandler> logger)
    : BaseCommandHandler<SyncProjectModuleCommand>
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public override async Task<Result> Handle(
        SyncProjectModuleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the business incubator
            var businessIncubator = await repository.GetByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken);
            if (businessIncubator is null)
            {
                return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), request.BusinessIncubatorExternalId.ToString()));
            }

            // Get the project with knowledge structure
            var project = await repository.GetProjectWithKnowledgeStructureByExternalIdAsync(request.ProjectExternalId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), request.ProjectExternalId.ToString()));
            }

            // Verify project belongs to business incubator
            if (project.BusinessIncubatorId != businessIncubator.Id)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), request.ProjectExternalId.ToString()));
            }

            // Get the project knowledge structure
            var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);
            if (knowledgeStructure is null)
            {
                return Failure(ResultErrorCodes.KnowledgeStructure_NotFound, ("ProjectKnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));
            }

            // Find the module
            var module = knowledgeStructure.ProjectModules
                .FirstOrDefault(m => m.Id == request.ModuleId);

            if (module is null)
            {
                return Failure(ResultErrorCodes.Module_NotFound, (nameof(request.ModuleId), request.ModuleId.ToString()));
            }

            // Check if the module has a source to sync from
            if (!module.SourceModuleId.HasValue)
            {
                return Failure(ResultErrorCodes.Unknown, ("Module", "El módulo no tiene una fuente para sincronizar"));
            }

            // This is a placeholder for the sync logic
            // The actual synchronization will be triggered by integration events
            // from the KnowledgeStructure domain when source entities are updated
            LogModuleSyncRequested(module.Id, module.SourceModuleId.Value);

            // For now, we'll just return success
            // In the future, this could trigger a more complex synchronization process
            return Success();
        }
        catch (Exception ex)
        {
            LogCommandFailed(ex.Message);
            return Failure(ResultErrorCodes.Unknown, ("Error", ex.Message));
        }
    }

    [LoggerMessage(EventId = 4041, Level = LogLevel.Information, Message = "Module sync requested for module {ModuleId} from source {SourceModuleId}")]
    private partial void LogModuleSyncRequested(long moduleId, long sourceModuleId);

    [LoggerMessage(EventId = 4042, Level = LogLevel.Error, Message = "Command failed: {Message}")]
    private partial void LogCommandFailed(string message);
}
