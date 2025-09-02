using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

/// <summary>
/// Command to delete a project within a Business Incubator.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The external ID of the Business Incubator.</param>
/// <param name="ProjectExternalId">The external ID of the project to delete.</param>
public record DeleteProjectCommand(Guid BusinessIncubatorExternalId, Guid ProjectExternalId)
    : IBaseRequest;

/// <summary>
/// Validator for <see cref="DeleteProjectCommand"/>.
/// </summary>
public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteProjectCommandValidator"/> class.
    /// </summary>
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorExternalId).NotEmpty();
        RuleFor(x => x.ProjectExternalId).NotEmpty();
    }
}

/// <summary>
/// Handler for <see cref="DeleteProjectCommand"/>.
/// </summary>
public class DeleteProjectCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<DeleteProjectCommand>
{
    /// <summary>
    /// Handles the deletion of a project within a Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID, including its projects.
    /// - Checks if the Business Incubator exists and is not deleted.
    /// - Deletes the specified project within the Business Incubator.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or is deleted, or if project deletion fails.
    /// </summary>
    public override async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var incubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound);
        }

        // Get the project to delete
        var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);

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
            project.SetDeleted(auditContext);
            repository.Update(project);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.Project_DeleteFailed, (nameof(DeleteProjectCommand), ex.Message));
        }
    }
}
