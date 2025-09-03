using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.Auth.Application.Commands;

public sealed record AssignUserToProjectCommand(
    string UserId,
    long ProjectId,
    long IncubatorId,
    string Role) : IBaseRequest;

public sealed class AssignUserToProjectCommandHandler(
    IAuthRepository authRepository,
    ITimeProvider timeProvider) : BaseCommandHandler<AssignUserToProjectCommand>
{
    public override async Task<Result> Handle(AssignUserToProjectCommand request, CancellationToken cancellationToken)
    {
        // Check if assignment already exists
        var existingAccess = await authRepository.GetUserProjectAccessAsync(
            request.UserId,
            request.ProjectId,
            cancellationToken);

        var currentTime = timeProvider.UtcNow;

        if (existingAccess is not null)
        {
            if (existingAccess.IsActive && existingAccess.Role == request.Role)
            {
                // Already has the same role and is active
                return Success();
            }

            // Update existing access
            if (existingAccess.IsActive)
            {
                existingAccess.UpdateRole(request.Role, currentTime);
            }
            else
            {
                existingAccess.Reactivate(request.Role, currentTime);
            }

            await authRepository.UpdateUserProjectAccessAsync(existingAccess, cancellationToken);
        }
        else
        {
            // Create new access
            var newAccess = UserProjectAccess.Create(
                request.UserId,
                request.ProjectId,
                request.IncubatorId,
                request.Role,
                currentTime);

            await authRepository.AddUserProjectAccessAsync(newAccess, cancellationToken);
        }

        // Save changes
        await authRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Success();
    }
}