using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.Auth.Application.Commands;

public sealed record AssignUserToIncubatorCommand(
    string UserId,
    long IncubatorId,
    string Role) : IBaseRequest;

public sealed class AssignUserToIncubatorCommandHandler(
    IAuthRepository authRepository,
    ITimeProvider timeProvider) : BaseCommandHandler<AssignUserToIncubatorCommand>
{
    public override async Task<Result> Handle(AssignUserToIncubatorCommand request, CancellationToken cancellationToken)
    {
        // Check if assignment already exists
        var existingAccess = await authRepository.GetUserIncubatorAccessAsync(
            request.UserId,
            request.IncubatorId,
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

            await authRepository.UpdateUserIncubatorAccessAsync(existingAccess, cancellationToken);
        }
        else
        {
            // Create new access
            var newAccess = UserIncubatorAccess.Create(
                request.UserId,
                request.IncubatorId,
                request.Role,
                currentTime);

            await authRepository.AddUserIncubatorAccessAsync(newAccess, cancellationToken);
        }

        // Save changes
        await authRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Success();
    }
}