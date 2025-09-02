using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

/// <summary>
/// Command to update an existing Project.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The external ID of the Business Incubator.</param>
/// <param name="ProjectExternalId">The external ID of the Project.</param>
/// <param name="Name">The updated name of the project.</param>
/// <param name="Description">The updated description of the project.</param>
/// <param name="Key">The updated key of the project.</param>
public record UpdateProjectCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    string Name,
    string Description,
    string Key) : IBaseRequest;

/// <summary>
/// Validator for <see cref="UpdateProjectCommand"/>.
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProjectCommandValidator"/> class.
    /// </summary>
    public UpdateProjectCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.BusinessIncubatorExternalId).NotEmpty();
        RuleFor(x => x.ProjectExternalId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(1000);
    }
}

/// <summary>
/// Handler for <see cref="UpdateProjectCommand"/>.
/// </summary>
public class UpdateProjectCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<UpdateProjectCommand>
{
    /// <summary>
    /// Handles the update of an existing Project.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID, including its projects.
    /// - Checks if the Business Incubator exists and is not deleted.
    /// - Checks if a project with the same name or key already exists within the Business Incubator, excluding the current project.
    /// - Updates the details of the project within the Business Incubator.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or is deleted, or if project update fails.
    /// </summary>
    public override async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var incubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), "Business Incubator not found or is deleted."));
        }

        // Get the project to update
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

        // Check for duplicate name (excluding current project)
        if (await repository.ProjectExistsWithNameAsync(incubator.Id, request.Name, project.Id, cancellationToken))
        {
            return Failure(ResultErrorCodes.Project_UpdateFailed, (nameof(request.Name), "Ya existe otro proyecto con el mismo nombre."));
        }

        // Check for duplicate key (excluding current project)
        if (await repository.ProjectExistsWithKeyAsync(incubator.Id, request.Key, project.Id, cancellationToken))
        {
            return Failure(ResultErrorCodes.Project_UpdateFailed, (nameof(request.Key), "Ya existe otro proyecto con la misma clave."));
        }

        try
        {
            project.Update(request.Name, request.Description, request.Key, auditContext);
            repository.Update(project);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.Project_UpdateFailed, (nameof(UpdateProjectCommand), ex.Message));
        }
    }
}
