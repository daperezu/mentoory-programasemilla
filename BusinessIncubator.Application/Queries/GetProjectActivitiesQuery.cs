using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get project activities.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="Limit">The maximum number of activities to return.</param>
public record GetProjectActivitiesQuery(string UserId, long ProjectId, int Limit = 10)
    : IBaseRequest<List<ProjectActivityDto>>;

/// <summary>
/// DTO for project activity information.
/// </summary>
public class ProjectActivityDto
{
    /// <summary>
    /// Gets or sets the activity category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the activity description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the activity timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the activity icon.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    public string IconColor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the activity details.
    /// </summary>
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Handler for GetProjectActivitiesQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProjectActivitiesQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetProjectActivitiesQueryHandler(
    IBusinessIncubatorRepository repository)
    : BaseCommandHandler<GetProjectActivitiesQuery, List<ProjectActivityDto>>
{
    /// <inheritdoc/>
    public override async Task<Result<List<ProjectActivityDto>>> Handle(
        GetProjectActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var activities = new List<ProjectActivityDto>();

        // Verify user has access to project
        var isParticipant = await repository.IsUserProjectParticipantAsync(
            request.ProjectId, request.UserId, cancellationToken);
        if (!isParticipant)
        {
            return Success(activities);
        }

        // Get recent form submissions for all users in the project
        var projectWithForms = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
        if (projectWithForms is not null)
        {
            var recentForms = projectWithForms.FormSubmissions
                .OrderByDescending(f => f.SubmittedAt ?? f.StartedAt)
                .Take(request.Limit / 2) // Half for forms
                .Select(f => new ProjectActivityDto
                {
                    Category = "Formulario",
                    Description = f.Status == Domain.Enums.ProjectFormSubmissionStatus.Submitted
                        ? "Formulario enviado para revisión"
                        : f.Status == Domain.Enums.ProjectFormSubmissionStatus.Approved
                            ? "Formulario aprobado"
                            : f.Status == Domain.Enums.ProjectFormSubmissionStatus.Rejected
                                ? "Formulario rechazado"
                                : "Formulario actualizado",
                    Timestamp = f.SubmittedAt ?? f.StartedAt,
                    Icon = f.Status == Domain.Enums.ProjectFormSubmissionStatus.Approved
                        ? "fas fa-check-circle"
                        : "fas fa-file-alt",
                    IconColor = f.Status == Domain.Enums.ProjectFormSubmissionStatus.Approved
                        ? "success"
                        : f.Status == Domain.Enums.ProjectFormSubmissionStatus.Rejected
                            ? "danger"
                            : "primary",
                    UserName = f.ParticipantUserId ?? "Sistema",
                    Details = $"Fase: {f.Phase}",
                })
                .ToList();

            activities.AddRange(recentForms);
        }

        // Get recent stage changes
        var project = await repository.GetProjectWithStagesAsync(request.ProjectId, cancellationToken);
        if (project is not null)
        {
            var recentStageChanges = project.ProjectStages
                .Where(s => s.UpdatedAt.HasValue)
                .OrderByDescending(s => s.UpdatedAt)
                .Take(request.Limit / 2) // Half for stages
                .Select(s => new ProjectActivityDto
                {
                    Category = "Etapa",
                    Description = s.IsActive
                        ? $"Etapa '{s.Title}' activada"
                        : $"Etapa '{s.Title}' actualizada",
                    Timestamp = s.UpdatedAt ?? s.CreatedAt,
                    Icon = "fas fa-tasks",
                    IconColor = s.IsActive ? "success" : "info",
                    UserName = s.UpdatedBy ?? "Sistema",
                    Details = s.EndDate != default
                        ? $"Fecha límite: {s.EndDate:dd/MM/yyyy}"
                        : string.Empty,
                });

            activities.AddRange(recentStageChanges);
        }

        // Sort all activities by timestamp and limit
        activities = activities
            .OrderByDescending(a => a.Timestamp)
            .Take(request.Limit)
            .ToList();

        return Success(activities);
    }
}