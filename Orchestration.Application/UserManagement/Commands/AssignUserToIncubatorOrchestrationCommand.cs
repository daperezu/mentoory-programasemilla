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

public record AssignUserToIncubatorOrchestrationCommand(
    string UserId,
    long IncubatorId,
    string Role) : LinaSys.Shared.Application.MediatR.IBaseRequest;

public class AssignUserToIncubatorOrchestrationCommandValidator : AbstractValidator<AssignUserToIncubatorOrchestrationCommand>
{
    private static readonly string[] ValidIncubatorRoles =
    [
        Roles.Coordinator,
        Roles.Administrator,
        Roles.Liaison
    ];

    public AssignUserToIncubatorOrchestrationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.IncubatorId)
            .GreaterThan(0).WithMessage("El ID de la incubadora debe ser mayor que cero.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es requerido.")
            .Must(role => ValidIncubatorRoles.Contains(role))
            .WithMessage($"El rol debe ser uno de los siguientes: {string.Join(", ", ValidIncubatorRoles)}");
    }
}

public class AssignUserToIncubatorOrchestrationCommandHandler(
    IMediator mediator,
    ILogger<AssignUserToIncubatorOrchestrationCommandHandler> logger)
    : BaseCommandHandler<AssignUserToIncubatorOrchestrationCommand>
{
    public override async Task<Result> Handle(AssignUserToIncubatorOrchestrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Starting incubator assignment for user {UserId} to incubator {IncubatorId} with role {Role}",
                request.UserId,
                request.IncubatorId,
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

            // 2. Verify incubator exists
            var incubatorQuery = new GetIncubatorByIdQuery(request.IncubatorId);
            var incubatorResult = await mediator.Send(incubatorQuery, cancellationToken);
            if (!incubatorResult.IsSuccess)
            {
                logger.LogWarning("Incubator {IncubatorId} not found", request.IncubatorId);
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(request.IncubatorId), "La incubadora no existe."));
            }

            var incubator = incubatorResult.Value!;

            // 3. Check if user has access to incubator
            var checkAccessQuery = new CheckUserAccessQuery(request.UserId, "incubator", request.IncubatorId);
            var hasAccess = await mediator.Send(checkAccessQuery, cancellationToken);

            // 4. Assign user to incubator with role
            var assignCommand = new AssignUserToIncubatorCommand(
                request.UserId,
                request.IncubatorId,
                request.Role);

            var assignResult = await mediator.Send(assignCommand, cancellationToken);
            if (!assignResult.IsSuccess)
            {
                logger.LogWarning(
                    "Failed to assign user {UserId} to incubator {IncubatorId} with role {Role}",
                    request.UserId,
                    request.IncubatorId,
                    request.Role);
                return assignResult;
            }

            logger.LogInformation(
                "Successfully assigned user {UserId} to incubator {IncubatorId} with role {Role}",
                request.UserId,
                request.IncubatorId,
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

            logger.LogInformation(
                "Successfully assigned user {UserId} to incubator {IncubatorId} with role {Role}",
                request.UserId,
                request.IncubatorId,
                request.Role);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error during incubator assignment for user {UserId} to incubator {IncubatorId}",
                request.UserId,
                request.IncubatorId);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al asignar el usuario a la incubadora."));
        }
    }
}