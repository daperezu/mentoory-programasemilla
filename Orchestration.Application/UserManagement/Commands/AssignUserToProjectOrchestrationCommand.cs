using FluentValidation;
using LinaSys.Auth.Application.Commands;
using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record AssignUserToProjectOrchestrationCommand(
    string UserId,
    Guid ProjectExternalId,
    string Role) : LinaSys.Shared.Application.MediatR.IBaseRequest;

public class AssignUserToProjectOrchestrationCommandValidator : AbstractValidator<AssignUserToProjectOrchestrationCommand>
{
    private static readonly string[] ValidProjectRoles =
    [
        Roles.Starter,
        Roles.Coordinator,
        Roles.Mentor,
        Roles.Guide,
        Roles.Facilitator
    ];

    public AssignUserToProjectOrchestrationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty().WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es requerido.")
            .Must(role => ValidProjectRoles.Contains(role))
            .WithMessage($"El rol debe ser uno de los siguientes: {string.Join(", ", ValidProjectRoles)}");
    }
}

public class AssignUserToProjectOrchestrationCommandHandler(
    IMediator mediator,
    ILogger<AssignUserToProjectOrchestrationCommandHandler> logger)
    : BaseCommandHandler<AssignUserToProjectOrchestrationCommand>
{
    public override async Task<Result> Handle(AssignUserToProjectOrchestrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Starting project assignment for user {UserId} to project {ProjectId} with role {Role}",
                request.UserId,
                request.ProjectExternalId,
                request.Role);

            // 1. Verify user exists
            var userQuery = new GetUserByIdQuery(request.UserId);
            var userResult = await mediator.Send(userQuery, cancellationToken);
            if (!userResult.IsSuccess)
            {
                logger.LogWarning("User {UserId} not found", request.UserId);
                return Failure(
                    ResultErrorCodes.User_NotFound,
                    (nameof(request.UserId), "El usuario no existe."));
            }

            // 2. Verify project exists and get its details
            var projectQuery = new GetProjectByExternalIdQuery(request.ProjectExternalId);
            var projectResult = await mediator.Send(projectQuery, cancellationToken);
            if (!projectResult.IsSuccess)
            {
                logger.LogWarning("Project {ProjectId} not found", request.ProjectExternalId);
                return Failure(
                    ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "El proyecto no existe."));
            }

            var project = projectResult.Value!;

            // 3. Check if user has access to project
            var checkAccessQuery = new CheckUserAccessQuery(request.UserId, "project", project.Id);
            var hasAccess = await mediator.Send(checkAccessQuery, cancellationToken);

            // 4. Assign user to project with role
            var assignCommand = new AssignUserToProjectCommand(
                request.UserId,
                project.Id,
                project.IncubatorId,
                request.Role);

            var assignResult = await mediator.Send(assignCommand, cancellationToken);
            if (!assignResult.IsSuccess)
            {
                logger.LogWarning(
                    "Failed to assign user {UserId} to project {ProjectId} with role {Role}",
                    request.UserId,
                    project.Id,
                    request.Role);
                return assignResult;
            }

            logger.LogInformation(
                "Successfully assigned user {UserId} to project {ProjectId} with role {Role}",
                request.UserId,
                project.Id,
                request.Role);

            // 5. Ensure user has the system role if not already assigned
            var rolesQuery = new GetUserRolesQuery(request.UserId);
            var rolesResult = await mediator.Send(rolesQuery, cancellationToken);

            if (rolesResult.IsSuccess && rolesResult.Value != null && !rolesResult.Value.Contains(request.Role))
            {
                var addRoleCommand = new AddUserToRoleCommand(request.UserId, request.Role);
                var roleResult = await mediator.Send(addRoleCommand, cancellationToken);

                if (!roleResult.IsSuccess)
                {
                    logger.LogWarning(
                        "Failed to add system role {Role} to user {UserId}",
                        request.Role,
                        request.UserId);
                }
                else
                {
                    logger.LogInformation(
                        "Added system role {Role} to user {UserId}",
                        request.Role,
                        request.UserId);
                }
            }

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error during project assignment for user {UserId} to project {ProjectId}",
                request.UserId,
                request.ProjectExternalId);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al asignar el usuario al proyecto."));
        }
    }
}