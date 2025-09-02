using System.Reflection;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Auth;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Web.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces authorization for commands and queries.
/// Works with RequiresPermissionAttribute to check permissions before execution.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandAuthorizationBehavior{TRequest,TResponse}"/> class.
/// </remarks>
/// <param name="authScopeProvider">The authorization scope provider.</param>
/// <param name="accessChecker">The access checker.</param>
/// <param name="logger">The logger.</param>
public class CommandAuthorizationBehavior<TRequest, TResponse>(
    IAuthScopeProvider authScopeProvider,
    IAccessChecker accessChecker,
    ILogger<CommandAuthorizationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{

    /// <inheritdoc/>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requiresPermission = request.GetType().GetCustomAttribute<CommandRequiresPermissionAttribute>();

        if (requiresPermission == null)
        {
            // No permission required, proceed
            return await next(cancellationToken).ConfigureAwait(false);
        }

        // Check if user is authenticated
        if (!authScopeProvider.IsAuthenticated())
        {
            logger.LogWarning("Unauthorized access attempt to {RequestType}", request.GetType().Name);
            return CreateUnauthorizedResponse();
        }

        var userId = authScopeProvider.GetCurrentUserId();
        var authorized = true;

        // Check required roles
        if (requiresPermission.RequiredRoles?.Length > 0)
        {
            var userRoles = authScopeProvider.GetCurrentUserRoles();
            authorized = requiresPermission.RequiredRoles.Any(role => userRoles.Contains(role));

            if (!authorized)
            {
                logger.LogWarning(
                    "User {UserId} lacks required roles for {RequestType}. Required: {RequiredRoles}, Has: {UserRoles}",
                    userId,
                    request.GetType().Name,
                    string.Join(", ", requiresPermission.RequiredRoles),
                    string.Join(", ", userRoles));
                return CreateForbiddenResponse("No tiene los permisos necesarios para realizar esta operación.");
            }
        }

        // Check project access
        if (requiresPermission.RequiresProjectAccess && !string.IsNullOrEmpty(requiresPermission.ProjectExternalIdProperty))
        {
            var projectExternalId = GetPropertyValue<Guid>(request, requiresPermission.ProjectExternalIdProperty);
            if (projectExternalId != Guid.Empty)
            {
                authorized = await accessChecker.HasProjectAccessAsync(userId, projectExternalId, cancellationToken).ConfigureAwait(false);

                if (!authorized)
                {
                    logger.LogWarning(
                        "User {UserId} lacks project access for {RequestType}. Project: {ProjectId}",
                        userId,
                        request.GetType().Name,
                        projectExternalId);
                    return CreateForbiddenResponse("No tiene acceso a este proyecto.");
                }
            }
        }

        // Check business incubator access
        if (requiresPermission.RequiresBusinessIncubatorAccess && !string.IsNullOrEmpty(requiresPermission.BusinessIncubatorExternalIdProperty))
        {
            var businessIncubatorExternalId = GetPropertyValue<Guid>(request, requiresPermission.BusinessIncubatorExternalIdProperty);
            if (businessIncubatorExternalId != Guid.Empty)
            {
                authorized = await accessChecker.HasBusinessIncubatorAccessAsync(userId, businessIncubatorExternalId, cancellationToken).ConfigureAwait(false);

                if (!authorized)
                {
                    logger.LogWarning(
                        "User {UserId} lacks business incubator access for {RequestType}. Incubator: {IncubatorId}",
                        userId,
                        request.GetType().Name,
                        businessIncubatorExternalId);
                    return CreateForbiddenResponse("No tiene acceso a esta incubadora de negocios.");
                }
            }
        }

        // Check specific permission types
        if (requiresPermission.PermissionType == PermissionType.ProjectCoordinator && !string.IsNullOrEmpty(requiresPermission.ProjectExternalIdProperty))
        {
            var projectExternalId = GetPropertyValue<Guid>(request, requiresPermission.ProjectExternalIdProperty);
            if (projectExternalId != Guid.Empty)
            {
                // Check for coordinator roles
                var coordinatorRoles = new[] { "Mentor", "Guide", "Liaison", "Facilitator", "Starter" };
                authorized = false;
                foreach (var role in coordinatorRoles)
                {
                    if (await accessChecker.HasProjectRoleAsync(userId, projectExternalId, role, cancellationToken).ConfigureAwait(false))
                    {
                        authorized = true;
                        break;
                    }
                }

                if (!authorized)
                {
                    logger.LogWarning(
                        "User {UserId} is not a coordinator for project {ProjectId} in {RequestType}",
                        userId,
                        projectExternalId,
                        request.GetType().Name);
                    return CreateForbiddenResponse("No tiene permisos de coordinador en este proyecto.");
                }
            }
        }

        if (authorized)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        return CreateForbiddenResponse("No tiene los permisos necesarios para realizar esta operación.");
    }

    private static T GetPropertyValue<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property == null)
        {
            return default!;
        }

        var value = property.GetValue(obj);
        return value is T typedValue ? typedValue : default!;
    }

    private TResponse CreateUnauthorizedResponse()
    {
        // Check if TResponse is Result or Result<T>
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(
                ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource,
                ("Authorization", "Debe iniciar sesión para realizar esta operación."));
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod("Failure", [typeof(ResultErrorCodes), typeof((string, string)[])]);

            var result = failureMethod!.Invoke(
                null,
                [
                    ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource,
                    new[] { ("Authorization", "Debe iniciar sesión para realizar esta operación.") },
                ]);

            return (TResponse)result!;
        }

        throw new UnauthorizedAccessException("Debe iniciar sesión para realizar esta operación.");
    }

    private TResponse CreateForbiddenResponse(string message)
    {
        // Check if TResponse is Result or Result<T>
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(
                ResultErrorCodes.Auth_RolesHasNoAccessToProtectedResource,
                ("Authorization", message));
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod("Failure", [typeof(ResultErrorCodes), typeof((string, string)[])]);

            var result = failureMethod!.Invoke(
                null,
                [
                    ResultErrorCodes.Auth_RolesHasNoAccessToProtectedResource,
                    new[] { ("Authorization", message) },
                ]);

            return (TResponse)result!;
        }

        throw new UnauthorizedAccessException(message);
    }
}
