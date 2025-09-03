using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

/// <summary>
/// Command to change the status of a Project inside a Business Incubator.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The external ID of the Business Incubator.</param>
/// <param name="ProjectExternalId">The external ID of the Project.</param>
/// <param name="Status">The new status of the Project.</param>
public record ChangeProjectStatusCommand(Guid BusinessIncubatorExternalId, Guid ProjectExternalId, ProjectStatus Status) : IBaseRequest;

/// <summary>
/// Validator for <see cref="ChangeProjectStatusCommand"/>.
/// </summary>
public class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeProjectStatusCommandValidator"/> class.
    /// </summary>
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorExternalId).NotEmpty();
        RuleFor(x => x.ProjectExternalId).NotEmpty();
    }
}

/// <summary>
/// Handler for <see cref="ChangeProjectStatusCommand"/>.
/// </summary>
public class ChangeProjectStatusCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext) : BaseCommandHandler<ChangeProjectStatusCommand>
{
    /// <summary>
    /// Handles the change of status for a project within a Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID, including its projects.
    /// - Checks if the Business Incubator exists and is not deleted.
    /// - Checks if the specified project exists and is not deleted.
    /// - Changes the status of the specified project.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator or project is not found or is deleted.
    /// </summary>
    public override async Task<Result> Handle(ChangeProjectStatusCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var incubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), "Business Incubator not found or is deleted."));
        }

        // Get the project directly
        var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);

        if (project is null || project.IsDeleted)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project not found or is deleted."));
        }

        // Verify the project belongs to the business incubator
        if (project.BusinessIncubatorId != incubator.Id)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project does not belong to the specified Business Incubator."));
        }

        try
        {
            project.ChangeStatus(request.Status, auditContext);

            // Update the project in the repository
            repository.Update(project);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.Project_ChangeStatusFailed, (nameof(ChangeProjectStatusCommand), ex.Message));
        }
    }
}
