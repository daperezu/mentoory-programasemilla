using LinaSys.Auth.Application.Queries;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.UserManagement.Application.Services;
using MediatR;

namespace LinaSys.Web.Services;

public class UserCrossDomainService(
    IMediator mediator,
    IAuthRepository authRepository,
    IBusinessIncubatorRepository businessIncubatorRepository,
    ILogger<UserCrossDomainService> logger) : IUserCrossDomainService
{
    public async Task<string?> GetUserEmailAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserEmailQuery(userId);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess ? result.Value : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching email for user {UserId}", userId);
            return null;
        }
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserRolesQuery(userId);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess ? result.Value! : [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching roles for user {UserId}", userId);
            return [];
        }
    }

    public async Task<IReadOnlyList<UserProjectInfo>> GetUserProjectsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projectAccesses = await authRepository.GetUserProjectAccessesAsync(userId, cancellationToken);

            var projectInfos = new List<UserProjectInfo>();

            foreach (var access in projectAccesses)
            {
                var project = await businessIncubatorRepository.GetProjectByIdAsync(access.ProjectId, cancellationToken);
                if (project is not null)
                {
                    projectInfos.Add(new UserProjectInfo(
                        access.ProjectId,
                        project.Name,
                        access.Role,
                        access.IsActive));
                }
            }

            return projectInfos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching projects for user {UserId}", userId);
            return [];
        }
    }

    public async Task<IReadOnlyList<UserIncubatorInfo>> GetUserIncubatorsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var incubatorAccesses = await authRepository.GetUserIncubatorAccessesAsync(userId, cancellationToken);

            var incubatorInfos = new List<UserIncubatorInfo>();

            foreach (var access in incubatorAccesses)
            {
                var incubator = await businessIncubatorRepository.GetByIdAsync(access.IncubatorId, cancellationToken);
                if (incubator is not null)
                {
                    incubatorInfos.Add(new UserIncubatorInfo(
                        access.IncubatorId,
                        incubator.Name,
                        access.Role,
                        access.IsActive));
                }
            }

            return incubatorInfos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching incubators for user {UserId}", userId);
            return [];
        }
    }
}
