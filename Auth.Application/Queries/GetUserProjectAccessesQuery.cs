using FluentValidation;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.Queries;

/// <summary>
/// Query to retrieve all project access records for a user.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetUserProjectAccessesQuery(string UserId) : IBaseRequest<List<UserProjectAccessDto>>;

/// <summary>
/// DTO for user project access information.
/// </summary>
public sealed class UserProjectAccessDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the incubator identifier.
    /// </summary>
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the access is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the last synchronization timestamp.
    /// </summary>
    public DateTime LastSyncedAt { get; set; }
}

/// <summary>
/// Validator for GetUserProjectAccessesQuery.
/// </summary>
public sealed class GetUserProjectAccessesQueryValidator : AbstractValidator<GetUserProjectAccessesQuery>
{
    public GetUserProjectAccessesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario no puede estar vacío.")
            .Must(BeValidGuid)
            .WithMessage("El ID del usuario debe ser un GUID válido.");
    }

    private bool BeValidGuid(string userId)
    {
        return Guid.TryParse(userId, out _);
    }
}

/// <summary>
/// Handler for GetUserProjectAccessesQuery.
/// </summary>
public sealed class GetUserProjectAccessesQueryHandler(
    IAuthRepository authRepository,
    ILogger<GetUserProjectAccessesQueryHandler> logger)
    : BaseCommandHandler<GetUserProjectAccessesQuery, List<UserProjectAccessDto>>
{
    /// <summary>
    /// Handles the GetUserProjectAccessesQuery.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of user project access records.</returns>
    public override async Task<Result<List<UserProjectAccessDto>>> Handle(
        GetUserProjectAccessesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving project accesses for user: {UserId}", request.UserId);

            var accessRecords = await authRepository.GetUserProjectAccessesAsync(request.UserId, cancellationToken);

            var dtos = accessRecords.Select(access => new UserProjectAccessDto
            {
                ProjectId = access.ProjectId,
                IncubatorId = access.IncubatorId,
                Role = access.Role,
                IsActive = access.IsActive,
                LastSyncedAt = access.LastSyncedAt,
            }).ToList();

            logger.LogInformation("Found {Count} project access records for user: {UserId}", dtos.Count, request.UserId);

            return Success(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving project accesses for user: {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.Auth_QueryFailed,
                (nameof(request.UserId), "Error interno al consultar los accesos a proyectos."));
        }
    }
}
