using FluentValidation;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record RemoveRolesFromUserOrchestrationCommand(
    string UserId,
    List<string> Roles) : IBaseRequest;

public class RemoveRolesFromUserOrchestrationCommandValidator : AbstractValidator<RemoveRolesFromUserOrchestrationCommand>
{
    private static readonly string[] ValidRoles =
    [
        Roles.Starter,
        Roles.Coordinator,
        Roles.Mentor,
        Roles.Guide,
        Roles.Facilitator,
        Roles.Liaison,
        Roles.Administrator,
        Roles.GlobalAdministrator
    ];

    public RemoveRolesFromUserOrchestrationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("Debe especificar al menos un rol.")
            .Must(roles => roles.All(r => ValidRoles.Contains(r)))
            .WithMessage($"Los roles deben ser válidos. Roles permitidos: {string.Join(", ", ValidRoles)}");

        RuleForEach(x => x.Roles)
            .NotEmpty().WithMessage("El nombre del rol no puede estar vacío.");
    }
}

public class RemoveRolesFromUserOrchestrationCommandHandler(
    IAuthRepository authRepository,
    ILogger<RemoveRolesFromUserOrchestrationCommandHandler> logger)
    : BaseCommandHandler<RemoveRolesFromUserOrchestrationCommand>
{
    public override async Task<Result> Handle(RemoveRolesFromUserOrchestrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting role removal for user {UserId}", request.UserId);

            // 1. Verify user exists
            var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User {UserId} not found", request.UserId);
                return Failure(
                    ResultErrorCodes.User_NotFound,
                    (nameof(request.UserId), "El usuario no existe."));
            }

            // 2. Get current user roles
            var currentRoles = await authRepository.GetRolesAsync(user, cancellationToken);
            logger.LogInformation("User {UserId} currently has roles: {Roles}", request.UserId, string.Join(", ", currentRoles));

            // 3. Identify roles to remove (that are actually assigned)
            var rolesToRemove = request.Roles.Intersect(currentRoles).ToList();

            if (!rolesToRemove.Any())
            {
                logger.LogInformation("User {UserId} doesn't have any of the requested roles to remove", request.UserId);
                return Success();
            }

            // 4. Validate business rules - prevent removing last admin role
            if (rolesToRemove.Contains(Roles.GlobalAdministrator))
            {
                // Check if user is the last global admin
                var globalAdminRole = await authRepository.GetUsersInRoleAsync(Roles.GlobalAdministrator, cancellationToken);
                if (globalAdminRole.Count == 1 && globalAdminRole.First().Id == request.UserId)
                {
                    logger.LogWarning("Cannot remove GlobalAdministrator role from user {UserId} - last admin in system", request.UserId);
                    return Failure(
                        ResultErrorCodes.Role_RemovalFailed,
                        (nameof(request.Roles), "No se puede remover el rol de Administrador Global del último administrador del sistema."));
                }
            }

            // 5. Remove roles
            var result = await authRepository.RemoveFromRolesAsync(user, rolesToRemove, cancellationToken);
            if (!result.Success)
            {
                var errors = string.Join("; ", result.Errors);
                logger.LogError("Failed to remove roles from user {UserId}: {Errors}", request.UserId, errors);
                return Failure(
                    ResultErrorCodes.Role_RemovalFailed,
                    (nameof(request.Roles), $"Error al remover roles: {errors}"));
            }

            logger.LogInformation(
                "Successfully removed roles [{Roles}] from user {UserId}",
                string.Join(", ", rolesToRemove),
                request.UserId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during role removal for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al remover roles."));
        }
    }
}