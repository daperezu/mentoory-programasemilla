using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

/// <summary>
/// Query to get pending forms for a user across all projects.
/// </summary>
public record GetPendingFormsQuery(string UserId) : IBaseRequest<List<PendingFormDto>>;

/// <summary>
/// DTO for pending form information.
/// </summary>
public class PendingFormDto
{
    /// <summary>
    /// Gets or sets the submission identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the submission external identifier.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the stage name.
    /// </summary>
    public string StageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form phase.
    /// </summary>
    public Domain.Enums.QuestionPhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the submission status.
    /// </summary>
    public Domain.Enums.ProjectFormSubmissionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last updated date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public double CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the urgency level (danger, warning, info).
    /// </summary>
    public string UrgencyLevel { get; set; } = "info";

    /// <summary>
    /// Gets the days remaining until due date.
    /// </summary>
    public int? DaysRemaining => DueDate.HasValue
        ? (DueDate.Value.Date - DateTime.UtcNow.Date).Days
        : null;
}

/// <summary>
/// Handler for GetPendingFormsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPendingFormsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetPendingFormsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetPendingFormsQuery, List<PendingFormDto>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<PendingFormDto>>> Handle(
        GetPendingFormsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all projects for the user
        var projects = await repository.GetProjectsByUserAsync(request.UserId, cancellationToken);

        var pendingForms = new List<PendingFormDto>();

        foreach (var project in projects.Where(p => !p.IsDeleted && p.Status == Domain.Enums.ProjectStatus.Active))
        {
            // Get project with form submissions
            var projectWithSubmissions = await repository.GetProjectWithFormSubmissionsAsync(project.Id, cancellationToken);

            if (projectWithSubmissions?.FormSubmissions == null)
            {
                continue;
            }

            // Get project with stages for stage information
            var projectWithStages = await repository.GetProjectWithStagesAsync(project.Id, cancellationToken);

            // Filter for pending forms (Draft or PendingReview status)
            var userPendingSubmissions = projectWithSubmissions.FormSubmissions
                .Where(s => s.ParticipantUserId == request.UserId &&
                       (s.Status == Domain.Enums.ProjectFormSubmissionStatus.Draft ||
                        s.Status == Domain.Enums.ProjectFormSubmissionStatus.Submitted))
                .ToList();

            foreach (var submission in userPendingSubmissions)
            {
                // Find the related stage
                var stage = projectWithStages?.ProjectStages?.FirstOrDefault(s => s.Id == submission.ProjectStageId);

                // Use the stored completion percentage from the submission
                var completionPercentage = submission.CompletionPercentage;

                // Determine urgency level based on stage end date
                var urgencyLevel = "info";
                if (stage?.EndDate != null)
                {
                    var daysRemaining = (stage.EndDate.Date - DateTime.UtcNow.Date).Days;
                    if (daysRemaining <= 0)
                    {
                        urgencyLevel = "danger";
                    }
                    else if (daysRemaining <= 3)
                    {
                        urgencyLevel = "warning";
                    }
                }

                var dto = new PendingFormDto
                {
                    Id = submission.Id,
                    ExternalId = submission.ExternalId,
                    ProjectName = project.Name,
                    ProjectId = project.Id,
                    StageName = stage?.Title ?? string.Empty,
                    Phase = submission.Phase,
                    Status = submission.Status,
                    DueDate = stage?.EndDate,
                    CreatedAt = submission.StartedAt,
                    UpdatedAt = submission.LastAutoSaveAt,
                    CompletionPercentage = completionPercentage,
                    UrgencyLevel = urgencyLevel
                };

                pendingForms.Add(dto);
            }
        }

        // Sort by urgency and due date
        pendingForms = pendingForms
            .OrderBy(f => f.UrgencyLevel switch { "danger" => 0, "warning" => 1, _ => 2 })
            .ThenBy(f => f.DueDate ?? DateTime.MaxValue)
            .ToList();

        return Success(pendingForms);
    }
}