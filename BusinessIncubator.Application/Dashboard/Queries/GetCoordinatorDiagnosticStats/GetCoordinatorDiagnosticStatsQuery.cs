using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorDiagnosticStats;

/// <summary>
/// Query to get diagnostic form statistics for a coordinator's project.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetCoordinatorDiagnosticStatsQuery(long ProjectId) : IBaseRequest<CoordinatorDiagnosticStatsDto>;

/// <summary>
/// DTO for coordinator diagnostic statistics.
/// </summary>
public class CoordinatorDiagnosticStatsDto
{
    /// <summary>
    /// Gets or sets the total number of forms.
    /// </summary>
    public int TotalForms { get; set; }

    /// <summary>
    /// Gets or sets the number of completed forms.
    /// </summary>
    public int CompletedForms { get; set; }

    /// <summary>
    /// Gets or sets the number of in-progress forms.
    /// </summary>
    public int InProgressForms { get; set; }

    /// <summary>
    /// Gets or sets the number of not started forms.
    /// </summary>
    public int NotStartedCount { get; set; }

    /// <summary>
    /// Gets or sets the completion rate percentage.
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// Gets or sets the average completion time in hours.
    /// </summary>
    public double AverageCompletionTime { get; set; }

    /// <summary>
    /// Gets or sets statistics by form type.
    /// </summary>
    public List<FormTypeStats> ByFormType { get; set; } = [];
}

/// <summary>
/// Statistics for a specific form type.
/// </summary>
public class FormTypeStats
{
    /// <summary>
    /// Gets or sets the form type name.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total count for this type.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the completed count for this type.
    /// </summary>
    public int Completed { get; set; }
}

/// <summary>
/// Handler for GetCoordinatorDiagnosticStatsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCoordinatorDiagnosticStatsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetCoordinatorDiagnosticStatsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetCoordinatorDiagnosticStatsQuery, CoordinatorDiagnosticStatsDto>
{

    /// <inheritdoc/>
    public override async Task<Result<CoordinatorDiagnosticStatsDto>> Handle(
        GetCoordinatorDiagnosticStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Get project with form submissions
        var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(GetCoordinatorDiagnosticStatsQuery), $"Project with ID {request.ProjectId} not found."));
        }

        // Use real form submission data
        var allSubmissions = project.FormSubmissions.ToList();
        var completedSubmissions = allSubmissions.Where(s => s.Status == ProjectFormSubmissionStatus.Submitted || s.Status == ProjectFormSubmissionStatus.Approved).ToList();
        var inProgressSubmissions = allSubmissions.Where(s => s.Status == ProjectFormSubmissionStatus.Draft).ToList();
        // Get all active project users to calculate not started count
        var projectWithUsers = await repository.GetProjectWithUsersAsync(request.ProjectId, cancellationToken);
        var activeUserCount = projectWithUsers?.ProjectUsers.Count(u => u.IsActive) ?? 0;
        var notStarted = Math.Max(0, activeUserCount - allSubmissions.Count);

        var totalForms = allSubmissions.Count + notStarted;
        var completedForms = completedSubmissions.Count;
        var inProgressForms = inProgressSubmissions.Count;

        // Calculate average completion time
        var completedWithTimes = completedSubmissions
            .Where(s => s.SubmittedAt.HasValue)
            .Select(s => (s.SubmittedAt!.Value - s.StartedAt).TotalHours)
            .ToList();
        var avgCompletionTime = completedWithTimes.Any() ? Math.Round(completedWithTimes.Average(), 1) : 0.0;

        // Group by form type (for now, all are "Diagnóstico" type)
        var formTypeGroups = new List<FormTypeStats>
        {
            new FormTypeStats
            {
                Type = "Diagnóstico",
                Count = totalForms,
                Completed = completedForms,
            },
        };

        var result = new CoordinatorDiagnosticStatsDto
        {
            TotalForms = totalForms,
            CompletedForms = completedForms,
            InProgressForms = inProgressForms,
            NotStartedCount = notStarted,
            CompletionRate = totalForms > 0 ? Math.Round((double)completedForms / totalForms * 100, 1) : 0,
            AverageCompletionTime = avgCompletionTime,
            ByFormType = formTypeGroups,
        };

        return Success(result);
    }
}
