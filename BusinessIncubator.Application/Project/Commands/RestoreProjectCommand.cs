using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

/// <summary>
/// Command to restore a deleted project within a Business Incubator.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The external ID of the Business Incubator.</param>
/// <param name="ProjectExternalId">The external ID of the project to restore.</param>
public record RestoreProjectCommand(Guid BusinessIncubatorExternalId, Guid ProjectExternalId) : IBaseRequest;

/// <summary>
/// Validator for <see cref="RestoreProjectCommand"/>.
/// </summary>
public class RestoreProjectCommandValidator : AbstractValidator<RestoreProjectCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreProjectCommandValidator"/> class.
    /// </summary>
    public RestoreProjectCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorExternalId).NotEmpty();
        RuleFor(x => x.ProjectExternalId).NotEmpty();
    }
}

/// <summary>
/// Handler for <see cref="RestoreProjectCommand"/>.
/// </summary>
public class RestoreProjectCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext) : BaseCommandHandler<RestoreProjectCommand>
{
    /// <summary>
    /// Handles the restoration of a deleted project within a Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID, including its projects.
    /// - Checks if the Business Incubator exists and is not deleted.
    /// - Restores the specified project within the Business Incubator.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or is deleted, or if project restoration fails.
    /// </summary>
    public override async Task<Result> Handle(RestoreProjectCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var incubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound);
        }

        // Get the project to restore - including deleted projects
        var project = await repository.GetProjectByExternalIdIncludingDeletedAsync(request.ProjectExternalId, cancellationToken);

        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project not found."));
        }

        // Verify the project belongs to the business incubator
        if (project.BusinessIncubatorId != incubator.Id)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project does not belong to the specified Business Incubator."));
        }

        try
        {
            project.SetRestored(auditContext);
            repository.Update(project);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.Project_RestoreFailed, (nameof(RestoreProjectCommand), ex.Message));
        }
    }
}
