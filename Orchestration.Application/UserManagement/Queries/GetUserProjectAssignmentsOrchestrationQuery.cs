using FluentValidation;
using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.UserManagement.Queries;

/// <summary>
/// Orchestration query to get a user's project assignments with enriched data.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetUserProjectAssignmentsOrchestrationQuery(string UserId) : IBaseRequest<List<UserProjectAssignmentDto>>;

/// <summary>
/// DTO for user project assignment with enriched data.
/// </summary>
public sealed class UserProjectAssignmentDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key.
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator identifier.
    /// </summary>
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator key.
    /// </summary>
    public string IncubatorKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the access is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date the user was assigned to the project.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last synchronization timestamp.
    /// </summary>
    public DateTime LastSyncedAt { get; set; }
}

/// <summary>
/// Validator for GetUserProjectAssignmentsOrchestrationQuery.
/// </summary>
public sealed class GetUserProjectAssignmentsOrchestrationQueryValidator : AbstractValidator<GetUserProjectAssignmentsOrchestrationQuery>
{
    public GetUserProjectAssignmentsOrchestrationQueryValidator()
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
/// Handler for GetUserProjectAssignmentsOrchestrationQuery.
/// </summary>
public sealed class GetUserProjectAssignmentsOrchestrationQueryHandler(
    IMediator mediator,
    ILogger<GetUserProjectAssignmentsOrchestrationQueryHandler> logger)
    : BaseCommandHandler<GetUserProjectAssignmentsOrchestrationQuery, List<UserProjectAssignmentDto>>
{
    /// <inheritdoc/>
    public override async Task<Result<List<UserProjectAssignmentDto>>> Handle(
        GetUserProjectAssignmentsOrchestrationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting project assignments for user {UserId}", request.UserId);

            // Step 1: Get user's project access records from Auth domain via query
            var accessQuery = new GetUserProjectAccessesQuery(request.UserId);
            var accessResult = await mediator.Send(accessQuery, cancellationToken);

            if (!accessResult.IsSuccess || accessResult.Value is null)
            {
                logger.LogError("Failed to retrieve project accesses for user {UserId}", request.UserId);
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(request.UserId), "Error al obtener información de accesos a proyectos."));
            }

            var projectAccesses = accessResult.Value;

            if (!projectAccesses.Any())
            {
                logger.LogInformation("No project assignments found for user {UserId}", request.UserId);
                return Success([]);
            }

            // Step 2: Get project details in batch
            var projectIds = projectAccesses.Select(a => a.ProjectId).Distinct().ToList();
            var projectsQuery = new GetProjectsByIdsQuery(projectIds);
            var projectsResult = await mediator.Send(projectsQuery, cancellationToken);

            if (!projectsResult.IsSuccess || projectsResult.Value is null)
            {
                logger.LogError("Failed to retrieve project details for user {UserId}", request.UserId);
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(request.UserId), "Error al obtener información de proyectos."));
            }

            // Step 3: Get incubator details in batch
            var incubatorIds = projectAccesses.Select(a => a.IncubatorId).Distinct().ToList();
            var incubatorsQuery = new GetIncubatorsByIdsQuery(incubatorIds);
            var incubatorsResult = await mediator.Send(incubatorsQuery, cancellationToken);

            if (!incubatorsResult.IsSuccess || incubatorsResult.Value is null)
            {
                logger.LogError("Failed to retrieve incubator details for user {UserId}", request.UserId);
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(request.UserId), "Error al obtener información de incubadoras."));
            }

            // Step 4: Create lookup dictionaries for efficient joining
            var projectsDict = projectsResult.Value.ToDictionary(p => p.Id);
            var incubatorsDict = incubatorsResult.Value.ToDictionary(i => i.Id);

            // Step 5: Combine access records with project and incubator details
            var assignments = new List<UserProjectAssignmentDto>();

            foreach (var access in projectAccesses)
            {
                // Skip if project or incubator no longer exists
                if (!projectsDict.TryGetValue(access.ProjectId, out var project))
                {
                    logger.LogWarning("Project {ProjectId} not found for user {UserId} assignment", access.ProjectId, request.UserId);
                    continue;
                }

                if (!incubatorsDict.TryGetValue(access.IncubatorId, out var incubator))
                {
                    logger.LogWarning("Incubator {IncubatorId} not found for user {UserId} assignment", access.IncubatorId, request.UserId);
                    continue;
                }

                // Skip deleted projects or incubators
                if (project.IsDeleted || incubator.IsDeleted)
                {
                    continue;
                }

                assignments.Add(new UserProjectAssignmentDto
                {
                    ProjectId = access.ProjectId,
                    ProjectName = project.Name,
                    ProjectKey = project.Key,
                    IncubatorId = access.IncubatorId,
                    IncubatorName = incubator.Name,
                    IncubatorKey = incubator.Key,
                    Role = access.Role,
                    IsActive = access.IsActive,
                    CreatedAt = access.LastSyncedAt, // UserProjectAccess doesn't inherit from AuditableEntity
                    LastSyncedAt = access.LastSyncedAt
                });
            }

            logger.LogInformation("Found {Count} project assignments for user {UserId}", assignments.Count, request.UserId);
            return Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving project assignments for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(request.UserId), "Error interno al consultar las asignaciones de proyectos."));
        }
    }
}
