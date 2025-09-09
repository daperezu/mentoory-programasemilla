using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get project details.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
public record GetProjectDetailsQuery(long ProjectId)
    : IBaseRequest<ProjectDetailsDto>;

/// <summary>
/// DTO for project details information.
/// </summary>
public class ProjectDetailsDto
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project external identifier.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current stage name.
    /// </summary>
    public string CurrentStage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stage end date.
    /// </summary>
    public DateTime? StageEndDate { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage.
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator identifier.
    /// </summary>
    public long? IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the incubator external identifier.
    /// </summary>
    public Guid? IncubatorExternalId { get; set; }

    /// <summary>
    /// Gets or sets the mentor name.
    /// </summary>
    public string MentorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mentor identifier.
    /// </summary>
    public string MentorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total number of stages.
    /// </summary>
    public int TotalStages { get; set; }

    /// <summary>
    /// Gets or sets the number of completed stages.
    /// </summary>
    public int CompletedStages { get; set; }
}

/// <summary>
/// Handler for GetProjectDetailsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProjectDetailsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
/// <param name="timeProvider">The time provider.</param>
public class GetProjectDetailsQueryHandler(
    IBusinessIncubatorRepository repository,
    TimeProvider timeProvider)
    : BaseCommandHandler<GetProjectDetailsQuery, ProjectDetailsDto>
{
    /// <inheritdoc/>
    public override async Task<Result<ProjectDetailsDto>> Handle(
        GetProjectDetailsQuery request,
        CancellationToken cancellationToken)
    {
        // Get project with stages
        var project = await repository.GetProjectWithStagesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.IsDeleted)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound,
                ("Project", "Proyecto no encontrado"));
        }

        // Get incubator information
        var incubator = await repository.GetByIdAsync(
            project.BusinessIncubatorId, cancellationToken);

        // Get project users to find mentor
        var projectWithUsers = await repository.GetProjectWithUsersAsync(
            project.Id, cancellationToken);

        string mentorName = "Sin mentor asignado";
        string mentorId = string.Empty;

        if (projectWithUsers is not null)
        {
            // ProjectUser doesn't have navigation to user details, just the userId
            // In a real scenario, you might need to call Auth service to get user details
            var mentor = projectWithUsers.ProjectUsers
                .FirstOrDefault(u => u.Role == "Mentor" && u.IsActive);

            if (mentor is not null)
            {
                mentorName = mentor.UserId; // We only have the ID, not the name
                mentorId = mentor.UserId;
            }
        }

        // Calculate current stage
        var currentDate = timeProvider.GetUtcNow().DateTime;
        var activeStage = project.ProjectStages
            .Where(s => s.IsActive && s.IsWithinPeriod(currentDate))
            .OrderBy(s => s.Id)
            .FirstOrDefault();

        // Calculate progress
        var completedStages = project.ProjectStages.Count(s =>
            s.EndDate < currentDate);
        var totalStages = project.ProjectStages.Count;
        var progress = totalStages > 0
            ? (int)((completedStages / (double)totalStages) * 100)
            : 0;

        var dto = new ProjectDetailsDto
        {
            ProjectId = project.Id,
            ExternalId = project.ExternalId,
            Name = project.Name,
            Description = project.Description ?? string.Empty,
            Status = project.Status.ToString(),
            CurrentStage = activeStage?.Title ?? "Sin etapa activa",
            StageEndDate = activeStage?.EndDate,
            Progress = progress,
            IncubatorName = incubator?.Name ?? "Sin incubadora",
            IncubatorId = incubator?.Id,
            IncubatorExternalId = incubator?.ExternalId,
            MentorName = mentorName,
            MentorId = mentorId,
            StartDate = project.CreatedAt,
            EndDate = project.ProjectStages
                .Where(s => s.EndDate != default)
                .Max(s => s.EndDate),
            TotalStages = totalStages,
            CompletedStages = completedStages,
        };

        return Success(dto);
    }
}