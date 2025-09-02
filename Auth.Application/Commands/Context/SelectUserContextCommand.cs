using LinaSys.Auth.Application.Queries.Context;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Auth.Domain.ValueObjects;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using MediatR;

namespace LinaSys.Auth.Application.Commands.Context;

/// <summary>
/// Command to select and save user context.
/// </summary>
public record SelectUserContextCommand(
    string UserId,
    string? Role,
    long? IncubatorId,
    long? ProjectId) : IBaseRequest<UserContext>;

/// <summary>
/// Handler for SelectUserContextCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SelectUserContextCommandHandler"/> class.
/// </remarks>
/// <param name="userContextRepository">The user context repository.</param>
/// <param name="mediator">The mediator.</param>
public class SelectUserContextCommandHandler(
    IUserContextRepository userContextRepository,
    IMediator mediator) : BaseCommandHandler<SelectUserContextCommand, UserContext>
{

    /// <inheritdoc/>
    public override async Task<Result<UserContext>> Handle(
        SelectUserContextCommand request,
        CancellationToken cancellationToken)
    {
        // Validate role is provided
        if (string.IsNullOrEmpty(request.Role))
        {
            return Failure(
                ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                (nameof(request.Role), "Debe seleccionar un rol"));
        }

        // Check if user is GlobalAdministrator or Administrator
        var isGlobalAdmin = request.Role == Roles.GlobalAdministrator;
        var isAdmin = request.Role == Roles.Administrator;

        // Apply role-based validation
        if (isGlobalAdmin)
        {
            // GlobalAdministrator: role is required, incubator and project are optional
            // No validation needed - they can work at any level
        }
        else if (isAdmin)
        {
            // Administrator: role + incubator required, no project
            if (!request.IncubatorId.HasValue)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(request.IncubatorId), "Debe seleccionar una incubadora"));
            }

            if (request.ProjectId.HasValue)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(request.ProjectId), "Administrador no requiere proyecto"));
            }
        }
        else
        {
            // Other roles: all three required
            if (!request.IncubatorId.HasValue || !request.ProjectId.HasValue)
            {
                return Failure(
                    ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    (nameof(SelectUserContextCommand), "Debe seleccionar incubadora y proyecto"));
            }
        }

        // Create context based on role
        UserContext context;
        if (isGlobalAdmin)
        {
            // Global Administrator can have optional incubator and project
            if (request is { IncubatorId: not null, ProjectId: not null })
            {
                // Global Admin with specific project context
                context = UserContext.CreateForGlobalAdministrator(
                    request.UserId,
                    request.Role,
                    request.IncubatorId.Value,
                    request.ProjectId.Value);
            }
            else if (request.IncubatorId.HasValue)
            {
                // Global Admin with specific incubator context (no project)
                context = UserContext.CreateForGlobalAdministrator(
                    request.UserId,
                    request.Role,
                    request.IncubatorId,
                    null);
            }
            else
            {
                // Global Admin with no specific context
                context = UserContext.CreateForGlobalAdministrator(
                    request.UserId,
                    request.Role);
            }
        }
        else if (isAdmin && request.IncubatorId.HasValue)
        {
            // For Administrator with only incubator
            context = UserContext.CreateForUser(
                request.UserId,
                request.Role,
                request.IncubatorId.Value,
                0);
        }
        else if (request is { IncubatorId: not null, ProjectId: not null })
        {
            // Other roles with full context
            context = UserContext.CreateForUser(
                request.UserId,
                request.Role,
                request.IncubatorId.Value,
                request.ProjectId.Value);
        }
        else
        {
            context = UserContext.CreateEmpty(request.UserId);
        }

        // Validate the context
        var validationResult = await mediator.Send(new ValidateUserContextQuery(context), cancellationToken);

        if (!validationResult.IsSuccess || !validationResult.Value)
        {
            return Failure(
                ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource,
                (nameof(SelectUserContextCommand), "No tiene permisos para el contexto seleccionado"));
        }

        // Save context preferences to database
        await SaveUserContextPreferences(context, cancellationToken);

        return Success(context);
    }

    private async Task SaveUserContextPreferences(UserContext context, CancellationToken cancellationToken)
    {
        var preferences = new UserContextPreferences
        {
            UserId = context.UserId,
            LastRole = context.Role,
            LastIncubatorId = context.IncubatorId,
            LastProjectId = context.ProjectId,
            UpdatedAt = DateTime.UtcNow
        };

        await userContextRepository.SaveUserContextPreferencesAsync(preferences, cancellationToken);
    }
}
