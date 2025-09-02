using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Queries;

/// <summary>
/// Query to list project invitations with pagination and filtering.
/// </summary>
/// <param name="ProjectExternalId">The external ID of the project.</param>
/// <param name="Start">The starting index for pagination.</param>
/// <param name="Length">The number of records to retrieve.</param>
/// <param name="Search">Global search term.</param>
/// <param name="OrderByColumn">Column to order by.</param>
/// <param name="OrderDirection">Order direction (asc/desc).</param>
/// <param name="Email">Filter by email.</param>
/// <param name="Status">Filter by invitation status.</param>
public record ListProjectInvitationsQuery(
    Guid ProjectExternalId,
    int Start = 0,
    int Length = 10,
    string? Search = null,
    string? OrderByColumn = null,
    string? OrderDirection = "asc",
    string? Email = null,
    string? Status = null) : IBaseRequest<ProjectInvitationsListResult>;

/// <summary>
/// Result containing paginated project invitations data.
/// </summary>
/// <param name="Data">The list of project invitations.</param>
/// <param name="TotalRecords">Total number of records without filtering.</param>
/// <param name="FilteredRecords">Total number of records after filtering.</param>
public record ProjectInvitationsListResult(
    List<ProjectInvitationListItem> Data,
    int TotalRecords,
    int FilteredRecords);

/// <summary>
/// Project invitation list item for display.
/// </summary>
/// <param name="Id">The invitation ID.</param>
/// <param name="Email">The invited user's email.</param>
/// <param name="FullName">The invited user's full name.</param>
/// <param name="Role">The role assigned to the user.</param>
/// <param name="Status">The current invitation status.</param>
/// <param name="CreatedAt">When the invitation was created.</param>
/// <param name="ExpiresAt">When the invitation expires.</param>
/// <param name="InvitationToken">The invitation token.</param>
public record ProjectInvitationListItem(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    string InvitationToken);

/// <summary>
/// Validator for the ListProjectInvitationsQuery.
/// </summary>
public class ListProjectInvitationsQueryValidator : AbstractValidator<ListProjectInvitationsQuery>
{
    public ListProjectInvitationsQueryValidator()
    {
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.Start)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El índice de inicio debe ser mayor o igual a 0.");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("La longitud debe estar entre 1 y 100.");

        When(x => !string.IsNullOrEmpty(x.OrderDirection), () =>
        {
            RuleFor(x => x.OrderDirection)
                .Must(x => x!.ToLowerInvariant() is "asc" or "desc")
                .WithMessage("La dirección de ordenamiento debe ser 'asc' o 'desc'.");
        });
    }
}

/// <summary>
/// Handler for the ListProjectInvitationsQuery.
/// </summary>
public class ListProjectInvitationsQueryHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    ILogger<ListProjectInvitationsQueryHandler> logger)
    : BaseCommandHandler<ListProjectInvitationsQuery, ProjectInvitationsListResult>
{
    /// <summary>
    /// Handles the ListProjectInvitationsQuery.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the paginated invitations list.</returns>
    public override async Task<Result<ProjectInvitationsListResult>> Handle(
        ListProjectInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project with its invitations
            var project = await businessIncubatorRepository
                .GetProjectWithInvitationsByExternalIdAsync(request.ProjectExternalId, cancellationToken);

            if (project is null)
            {
                logger.LogWarning("Project with external ID {ProjectId} not found", request.ProjectExternalId);
                return Failure(
                    ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "El proyecto no fue encontrado."));
            }

            // Get the invitations using the read-only collection
            var invitations = project.ProjectInvitations;

            // Apply filtering
            var filteredInvitations = ApplyFiltering(invitations, request);

            // Get total counts
            var totalRecords = invitations.Count;
            var filteredRecords = filteredInvitations.Count();

            // Apply ordering
            var orderedInvitations = ApplyOrdering(filteredInvitations, request);

            // Apply pagination
            var paginatedInvitations = orderedInvitations
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            // Map to result items
            var resultItems = paginatedInvitations.Select(invitation => new ProjectInvitationListItem(
                Id: invitation.ExternalId,
                Email: invitation.Email,
                FullName: invitation.FullName,
                Role: invitation.Role,
                Status: invitation.Status.ToString(),
                CreatedAt: invitation.CreatedAt,
                ExpiresAt: invitation.ExpiresAt,
                InvitationToken: invitation.InvitationToken)).ToList();

            var result = new ProjectInvitationsListResult(
                Data: resultItems,
                TotalRecords: totalRecords,
                FilteredRecords: filteredRecords);

            logger.LogInformation(
                "Listed {Count} invitations for project {ProjectId} (page {Page}, size {Size})",
                resultItems.Count,
                request.ProjectExternalId,
                (request.Start / request.Length) + 1,
                request.Length);

            return Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing invitations for project {ProjectId}", request.ProjectExternalId);
            return Failure(
                ResultErrorCodes.Project_ProcessingFailed,
                (nameof(request), "Error interno al obtener las invitaciones."));
        }
    }

    private static IEnumerable<Domain.Aggregates.BusinessIncubator.ProjectInvitation> ApplyFiltering(
        IEnumerable<Domain.Aggregates.BusinessIncubator.ProjectInvitation> invitations,
        ListProjectInvitationsQuery request)
    {
        var filtered = invitations.AsEnumerable();

        // Apply email filter
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            filtered = filtered.Where(i => i.Email.Contains(request.Email, StringComparison.OrdinalIgnoreCase));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            filtered = filtered.Where(i => i.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        }

        // Apply global search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            filtered = filtered.Where(i =>
                i.Email.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                i.FullName.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                i.Role.Contains(request.Search, StringComparison.OrdinalIgnoreCase));
        }

        return filtered;
    }

    private static IEnumerable<Domain.Aggregates.BusinessIncubator.ProjectInvitation> ApplyOrdering(
        IEnumerable<Domain.Aggregates.BusinessIncubator.ProjectInvitation> invitations,
        ListProjectInvitationsQuery request)
    {
        var isDescending = request.OrderDirection?.ToLowerInvariant() == "desc";

        return request.OrderByColumn?.ToLowerInvariant() switch
        {
            "email" => isDescending
                ? invitations.OrderByDescending(i => i.Email)
                : invitations.OrderBy(i => i.Email),
            "fullname" => isDescending
                ? invitations.OrderByDescending(i => i.FullName)
                : invitations.OrderBy(i => i.FullName),
            "role" => isDescending
                ? invitations.OrderByDescending(i => i.Role)
                : invitations.OrderBy(i => i.Role),
            "status" => isDescending
                ? invitations.OrderByDescending(i => i.Status)
                : invitations.OrderBy(i => i.Status),
            "createdat" => isDescending
                ? invitations.OrderByDescending(i => i.CreatedAt)
                : invitations.OrderBy(i => i.CreatedAt),
            "expiresat" => isDescending
                ? invitations.OrderByDescending(i => i.ExpiresAt)
                : invitations.OrderBy(i => i.ExpiresAt),
            _ => isDescending
                ? invitations.OrderByDescending(i => i.CreatedAt)
                : invitations.OrderBy(i => i.CreatedAt),
        };
    }
}
