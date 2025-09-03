using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.ClearProjectKnowledgeStructure;

/// <summary>
/// Handler for ClearProjectKnowledgeStructureCommand.
/// </summary>
public sealed partial class ClearProjectKnowledgeStructureCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    ILogger<ClearProjectKnowledgeStructureCommandHandler> logger)
    : BaseCommandHandler<ClearProjectKnowledgeStructureCommand>
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public override async Task<Result> Handle(
        ClearProjectKnowledgeStructureCommand request,
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

            // Clear the knowledge structure
            project.ClearKnowledgeStructure(auditContext);

            // Update the project
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            LogKnowledgeStructureCleared(request.ProjectExternalId);
            return Success();
        }
        catch (Exception ex)
        {
            LogCommandFailed(ex.Message);
            return Failure(ResultErrorCodes.Unknown, ("Error", ex.Message));
        }
    }

    [LoggerMessage(EventId = 4031, Level = LogLevel.Information, Message = "Knowledge structure cleared for project {ProjectExternalId}")]
    private partial void LogKnowledgeStructureCleared(Guid projectExternalId);

    [LoggerMessage(EventId = 4032, Level = LogLevel.Error, Message = "Command failed: {Message}")]
    private partial void LogCommandFailed(string message);
}