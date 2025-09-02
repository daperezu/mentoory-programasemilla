using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get projects for a participant (Starter role user).
/// </summary>
public record GetParticipantProjectsQuery(string UserId) : IBaseRequest<List<ParticipantProjectDto>>;

/// <summary>
/// DTO for participant project information.
/// </summary>
public class ParticipantProjectDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the project external identifier.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current stage name.
    /// </summary>
    public string? CurrentStageName { get; set; }

    /// <summary>
    /// Gets or sets the current stage order.
    /// </summary>
    public int CurrentStageOrder { get; set; }

    /// <summary>
    /// Gets or sets the total number of stages.
    /// </summary>
    public int TotalStages { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage.
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the user joined the project.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of pending forms.
    /// </summary>
    public int PendingFormsCount { get; set; }

    /// <summary>
    /// Gets or sets the project status.
    /// </summary>
    public string Status { get; set; } = "active";
}

/// <summary>
/// Handler for GetParticipantProjectsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetParticipantProjectsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetParticipantProjectsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetParticipantProjectsQuery, List<ParticipantProjectDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<ParticipantProjectDto>>> Handle(
        GetParticipantProjectsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all projects for the user
        var projects = await repository.GetProjectsByUserAsync(request.UserId, cancellationToken);

        var dtos = new List<ParticipantProjectDto>();

        foreach (var project in projects.Where(p => !p.IsDeleted))
        {
            // Get project with stages to determine current stage
            var projectWithStages = await repository.GetProjectWithStagesAsync(project.Id, cancellationToken);

            // Get project with form submissions to count pending forms
            var projectWithSubmissions = await repository.GetProjectWithFormSubmissionsAsync(project.Id, cancellationToken);

            // Get the incubator for the project
            var incubator = await repository.GetByProjectIdAsync(project.Id, cancellationToken);

            // Find the user's role in the project
            var projectUser = project.ProjectUsers.FirstOrDefault(u => u.UserId == request.UserId && u.IsActive);

            if (projectUser == null)
            {
                continue; // User is not active in this project
            }

            // Calculate current stage information
            var currentStage = projectWithStages?.ProjectStages
                ?.Where(s => s.IsActive)
                ?.FirstOrDefault();

            var totalStages = projectWithStages?.ProjectStages?.Count ?? 0;

            // Calculate progress based on stage dates if current stage exists
            var progressPercentage = 0.0;
            if (currentStage is not null && totalStages > 0)
            {
                var completedStages = projectWithStages?.ProjectStages?
                    .Count(s => s.EndDate < DateTime.UtcNow) ?? 0;
                progressPercentage = (double)completedStages / totalStages * 100;
            }

            // Count pending forms (submissions in draft or pending review status)
            var pendingFormsCount = projectWithSubmissions?.FormSubmissions
                ?.Count(s => s.ParticipantUserId == request.UserId &&
                       (s.Status == Domain.Enums.ProjectFormSubmissionStatus.Draft ||
                        s.Status == Domain.Enums.ProjectFormSubmissionStatus.Submitted)) ?? 0;

            var dto = new ParticipantProjectDto
            {
                Id = project.Id,
                ExternalId = project.ExternalId,
                Name = project.Name,
                Key = project.Key,
                CurrentStageName = currentStage?.Title,
                CurrentStageOrder = 0, // Not available without explicit order property
                TotalStages = totalStages,
                ProgressPercentage = progressPercentage,
                IncubatorName = incubator?.Name ?? string.Empty,
                JoinedAt = projectUser.JoinedAt,
                Role = projectUser.Role,
                PendingFormsCount = pendingFormsCount,
                Status = project.Status == Domain.Enums.ProjectStatus.Active ? "active" : "inactive"
            };

            dtos.Add(dto);
        }

        // Sort by most recent joined date
        dtos = dtos.OrderByDescending(p => p.JoinedAt).ToList();

        return Success(dtos);
    }
}