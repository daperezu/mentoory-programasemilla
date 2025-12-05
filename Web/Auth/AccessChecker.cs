using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Web.Services;

namespace LinaSys.Web.Auth;

/// <summary>
/// Implementation of IAccessChecker using CQRS for authorization.
/// Provides project and incubator access validation through queries and commands.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AccessChecker"/> class.
/// </remarks>
/// <param name="mediatorExecutor">The mediator executor for CQRS operations.</param>
/// <param name="logger">The logger.</param>
public class AccessChecker(
    MediatorExecutor mediatorExecutor,
    ILogger<AccessChecker> logger) : IAccessChecker
{

    /// <inheritdoc/>
    public async Task<bool> HasProjectAccessAsync(string userId, Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the internal project ID from the external ID
            var projectQuery = new GetProjectByExternalIdQuery(projectExternalId);
            var projectResult = await mediatorExecutor.SendOrThrowAsync(projectQuery, cancellationToken).ConfigureAwait(false);

            // Check user access to the project with the internal ID
            var accessQuery = new CheckUserAccessQuery(userId, "project", projectResult.Id);
            var hasAccess = await mediatorExecutor.SendOrThrowAsync(accessQuery, cancellationToken).ConfigureAwait(false);

            logger.LogDebug("User {UserId} access to project {ProjectId}: {HasAccess}", userId, projectResult.Id, hasAccess);
            return hasAccess;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking project access for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HasBusinessIncubatorAccessAsync(string userId, Guid businessIncubatorExternalId, CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the internal incubator ID from the external ID
            var incubatorQuery = new GetIncubatorByExternalIdQuery(businessIncubatorExternalId);
            var incubatorResult = await mediatorExecutor.SendOrThrowAsync(incubatorQuery, cancellationToken).ConfigureAwait(false);

            // Check user access to the incubator with the internal ID
            var accessQuery = new CheckUserAccessQuery(userId, "incubator", incubatorResult.Id);
            var hasAccess = await mediatorExecutor.SendOrThrowAsync(accessQuery, cancellationToken).ConfigureAwait(false);

            logger.LogDebug("User {UserId} access to incubator {IncubatorId}: {HasAccess}", userId, incubatorResult.Id, hasAccess);
            return hasAccess;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking incubator access for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HasProjectAccessAsync(string userId, long projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the CheckUserAccessQuery with internal project ID
            var accessQuery = new CheckUserAccessQuery(userId, "project", projectId);
            var hasAccess = await mediatorExecutor.SendOrThrowAsync(accessQuery, cancellationToken).ConfigureAwait(false);

            logger.LogDebug("User {UserId} access to project {ProjectId}: {HasAccess}", userId, projectId, hasAccess);
            return hasAccess;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking project access for user {UserId} and project {ProjectId}", userId, projectId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HasBusinessIncubatorAccessAsync(string userId, long businessIncubatorId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the CheckUserAccessQuery with internal incubator ID
            var accessQuery = new CheckUserAccessQuery(userId, "incubator", businessIncubatorId);
            var hasAccess = await mediatorExecutor.SendOrThrowAsync(accessQuery, cancellationToken).ConfigureAwait(false);

            logger.LogDebug("User {UserId} access to incubator {IncubatorId}: {HasAccess}", userId, businessIncubatorId, hasAccess);
            return hasAccess;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking incubator access for user {UserId} and incubator {IncubatorId}", userId, businessIncubatorId);
            return false;
        }
    }
}
