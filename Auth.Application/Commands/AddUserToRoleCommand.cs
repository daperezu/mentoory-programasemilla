using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Commands;

public sealed record AddUserToRoleCommand(string UserId, string RoleName) : IBaseRequest;

public sealed class AddUserToRoleCommandHandler(
    IAuthRepository authRepository) : BaseCommandHandler<AddUserToRoleCommand>
{
    public override async Task<Result> Handle(AddUserToRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await authRepository.FindUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Failure(ResultErrorCodes.User_NotFound, ("UserId", "Usuario no encontrado"));
        }

        // Check if user already has the role
        var userRoles = await authRepository.GetRolesAsync(user, cancellationToken);
        if (userRoles.Contains(request.RoleName))
        {
            return Success(); // Already has the role
        }

        // Add the role
        var (success, errors) = await authRepository.AddToRoleAsync(user, request.RoleName, cancellationToken);
        if (!success)
        {
            var errorArray = errors.Select(e => ("Role", e)).ToArray();
            return Failure(ResultErrorCodes.GenericError, errorArray);
        }

        return Success();
    }
}