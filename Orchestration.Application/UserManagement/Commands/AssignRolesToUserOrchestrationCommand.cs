using FluentValidation;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Commands;

public record AssignRolesToUserOrchestrationCommand(
    string UserId,
    List<string> Roles) : IBaseRequest;

public class AssignRolesToUserOrchestrationCommandValidator : AbstractValidator<AssignRolesToUserOrchestrationCommand>
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

    public AssignRolesToUserOrchestrationCommandValidator()
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

public class AssignRolesToUserOrchestrationCommandHandler(
    IAuthRepository authRepository,
    ILogger<AssignRolesToUserOrchestrationCommandHandler> logger)
    : BaseCommandHandler<AssignRolesToUserOrchestrationCommand>
{
    public override async Task<Result> Handle(AssignRolesToUserOrchestrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting role assignment for user {UserId}", request.UserId);

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

            // 3. Identify roles to add (not already assigned)
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();

            if (!rolesToAdd.Any())
            {
                logger.LogInformation("User {UserId} already has all requested roles", request.UserId);
                return Success();
            }

            // 4. Verify all roles exist in the system
            foreach (var roleName in rolesToAdd)
            {
                var roleExists = await authRepository.RoleExistsAsync(roleName, cancellationToken);
                if (!roleExists)
                {
                    logger.LogError("Role {RoleName} does not exist in the system", roleName);
                    return Failure(
                        ResultErrorCodes.Role_NotFound,
                        (nameof(request.Roles), $"El rol '{roleName}' no existe en el sistema."));
                }
            }

            // 5. Assign new roles
            var result = await authRepository.AddToRolesAsync(user, rolesToAdd, cancellationToken);
            if (!result.Success)
            {
                var errors = string.Join("; ", result.Errors);
                logger.LogError("Failed to assign roles to user {UserId}: {Errors}", request.UserId, errors);
                return Failure(
                    ResultErrorCodes.Role_AssignmentFailed,
                    (nameof(request.Roles), $"Error al asignar roles: {errors}"));
            }

            logger.LogInformation(
                "Successfully assigned roles [{Roles}] to user {UserId}",
                string.Join(", ", rolesToAdd),
                request.UserId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during role assignment for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Unknown,
                (nameof(request), "Error interno al asignar roles."));
        }
    }
}