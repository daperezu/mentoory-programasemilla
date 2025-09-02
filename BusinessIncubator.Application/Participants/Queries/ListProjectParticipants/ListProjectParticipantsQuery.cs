using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.AspNetCore.Identity;
using LinaSys.Auth.Domain.AggregatesModel.User;

namespace LinaSys.BusinessIncubator.Application.Participants.Queries.ListProjectParticipants;

/// <summary>
/// Query to get participants list for a project with DataTables support.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record ListProjectParticipantsQuery(
    long ProjectId,
    int Start,
    int Length,
    string? SearchValue = null,
    string? RoleFilter = null,
    string? StatusFilter = null) : IBaseRequest<ListProjectParticipantsDto>;

/// <summary>
/// DTO for project participants list.
/// </summary>
public class ListProjectParticipantsDto
{
    /// <summary>
    /// Gets or sets the total number of records.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the filtered record count.
    /// </summary>
    public int FilteredRecords { get; set; }

    /// <summary>
    /// Gets or sets the participant data.
    /// </summary>
    public List<ParticipantItem> Participants { get; set; } = [];
}

/// <summary>
/// Individual participant item.
/// </summary>
public class ParticipantItem
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identification number.
    /// </summary>
    public string IdentificationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant role.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the join date.
    /// </summary>
    public string JoinedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the active status.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the invitation status.
    /// </summary>
    public string InvitationStatus { get; set; } = "Aceptada";

    /// <summary>
    /// Gets or sets the last activity date.
    /// </summary>
    public string? LastActivity { get; set; }

    /// <summary>
    /// Gets or sets the form completion status.
    /// </summary>
    public string FormStatus { get; set; } = "Pendiente";
}

/// <summary>
/// Handler for ListProjectParticipantsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ListProjectParticipantsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
/// <param name="userManager">The user manager.</param>
public class ListProjectParticipantsQueryHandler(
    IBusinessIncubatorRepository repository,
    UserManager<User> userManager) : BaseCommandHandler<ListProjectParticipantsQuery, ListProjectParticipantsDto>
{

    /// <inheritdoc/>
    public override async Task<Result<ListProjectParticipantsDto>> Handle(
        ListProjectParticipantsQuery request,
        CancellationToken cancellationToken)
    {
        // Get project with users and form submissions
        var project = await repository.GetProjectWithUsersAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(ListProjectParticipantsQuery), $"Project with ID {request.ProjectId} not found."));
        }

        // Log total users found
        var totalProjectUsers = project.ProjectUsers?.Count ?? 0;
        var activeProjectUsers = project.ProjectUsers?.Where(pu => pu.IsActive).Count() ?? 0;

        // Note: We'd log here if we had access to a logger
        // Logger.LogInformation("Project {ProjectId} has {TotalUsers} total users, {ActiveUsers} active users",
        //     request.ProjectId, totalProjectUsers, activeProjectUsers);
        var projectWithSubmissions = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
        var participants = new List<ParticipantItem>();

        // Get all project users and load their details
        // Temporarily showing all users (active and inactive) for debugging
        if (project.ProjectUsers != null)
        {
            foreach (var projectUser in project.ProjectUsers)
        {
            var user = await userManager.FindByIdAsync(projectUser.UserId);
            if (user is null)
            {
                continue;
            }

            // Check form submission status
            var submission = projectWithSubmissions?.FormSubmissions
                .FirstOrDefault(s => s.ParticipantUserId == projectUser.UserId);

            var formStatus = submission?.Status switch
            {
                Domain.Enums.ProjectFormSubmissionStatus.Draft => "En progreso",
                Domain.Enums.ProjectFormSubmissionStatus.Submitted => "Enviado",
                Domain.Enums.ProjectFormSubmissionStatus.Approved => "Aprobado",
                Domain.Enums.ProjectFormSubmissionStatus.Rejected => "Rechazado",
                _ => "Pendiente"
            };

            var participantItem = new ParticipantItem
            {
                UserId = projectUser.UserId,
                FullName = $"{user.Email?.Split('@')[0] ?? "Usuario"} - {user.PhoneNumber ?? "Sin teléfono"}",
                Email = user.Email ?? string.Empty,
                IdentificationNumber = user.UserName ?? string.Empty, // UserName stores the identification number
                Role = projectUser.Role,
                JoinedAt = projectUser.JoinedAt.ToString("yyyy-MM-dd"),
                IsActive = projectUser.IsActive,
                InvitationStatus = "Aceptada", // If they're in ProjectUsers, invitation was accepted
                LastActivity = submission?.SubmittedAt?.ToString("yyyy-MM-dd HH:mm") ?? submission?.ApprovedAt?.ToString("yyyy-MM-dd HH:mm"),
                FormStatus = formStatus
            };

            participants.Add(participantItem);
            }
        }

        // Apply filters
        var filteredParticipants = participants.AsEnumerable();

        if (!string.IsNullOrEmpty(request.SearchValue))
        {
            var searchTerm = request.SearchValue.ToLowerInvariant();
            filteredParticipants = filteredParticipants.Where(p =>
                p.FullName.ToLowerInvariant().Contains(searchTerm) ||
                p.Email.ToLowerInvariant().Contains(searchTerm) ||
                p.Role.ToLowerInvariant().Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(request.RoleFilter))
        {
            filteredParticipants = filteredParticipants.Where(p => p.Role == request.RoleFilter);
        }

        if (!string.IsNullOrEmpty(request.StatusFilter))
        {
            filteredParticipants = filteredParticipants.Where(p =>
                request.StatusFilter switch
                {
                    "active" => p.IsActive,
                    "inactive" => !p.IsActive,
                    _ => true
                });
        }

        var filteredList = filteredParticipants.ToList();
        var totalFiltered = filteredList.Count;

        // Apply pagination
        var pagedParticipants = filteredList
            .Skip(request.Start)
            .Take(request.Length)
            .ToList();

        var result = new ListProjectParticipantsDto
        {
            TotalRecords = participants.Count,
            FilteredRecords = totalFiltered,
            Participants = pagedParticipants
        };

        return Success(result);
    }
}
