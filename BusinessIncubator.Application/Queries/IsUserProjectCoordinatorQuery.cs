using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to verify if a user is a coordinator/admin for a specific project.
/// </summary>
public record IsUserProjectCoordinatorQuery(Guid ProjectExternalId, string UserId) : IBaseRequest<bool>;

/// <summary>
/// Handler for IsUserProjectCoordinatorQuery.
/// </summary>
public class IsUserProjectCoordinatorQueryHandler(IBusinessIncubatorRepository repository)
    : BaseCommandHandler<IsUserProjectCoordinatorQuery, bool>
{
    public override async Task<Result<bool>> Handle(
        IsUserProjectCoordinatorQuery request,
        CancellationToken cancellationToken)
    {
        // First get the project by external ID
        var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
        if (project is null)
        {
            return Success(false);
        }

        // Check if user has coordinator access
        var isCoordinator = await repository.IsUserProjectCoordinatorAsync(
            project.Id,
            request.UserId,
            cancellationToken);

        return Success(isCoordinator);
    }
}