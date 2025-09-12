using LinaSys.Auth.Application.Queries.GetUsersByIds;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorDashboardCompleteData;

/// <summary>
/// Handler for GetCoordinatorDashboardCompleteDataQuery.
/// </summary>
public class GetCoordinatorDashboardCompleteDataQueryHandler(
    IBusinessIncubatorRepository repository,
    IMediator mediator,
    IMemoryCache cache,
    ITimeProvider timeProvider,
    ILogger<GetCoordinatorDashboardCompleteDataQueryHandler> logger)
    : BaseCommandHandler<GetCoordinatorDashboardCompleteDataQuery, CoordinatorDashboardCompleteDto>
{
    /// <inheritdoc/>
    public override async Task<Result<CoordinatorDashboardCompleteDto>> Handle(
        GetCoordinatorDashboardCompleteDataQuery request,
        CancellationToken cancellationToken)
    {
        // Try to get from cache first
        var cacheKey = $"coordinator_dashboard_complete_{request.ProjectId}_{request.CoordinatorUserId}";
        if (cache.TryGetValue<CoordinatorDashboardCompleteDto>(cacheKey, out var cachedResult))
        {
            logger.LogDebug("Returning cached dashboard data for project {ProjectId}", request.ProjectId);
            return Success(cachedResult!);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Get complete dashboard data in a single optimized query
        var dashboardData = await repository.GetProjectDashboardDataAsync(
            request.ProjectId,
            timeProvider.UtcNow,
            request.DateRangeStart,
            cancellationToken);

        if (dashboardData is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(GetCoordinatorDashboardCompleteDataQuery), $"Project with ID {request.ProjectId} not found."));
        }

        // Collect all user IDs for batch loading
        var allUserIds = dashboardData.AllUserIds.Distinct().ToList();

        // Batch load all users to avoid N+1 queries
        var getUsersResult = await mediator.Send(
            new GetUsersByIdsQuery(allUserIds),
            cancellationToken);

        var userLookup = getUsersResult.IsSuccess && getUsersResult.Value != null
            ? getUsersResult.Value
            : new Dictionary<string, UserBasicInfoDto>();

        // Build the complete DTO
        var result = new CoordinatorDashboardCompleteDto
        {
            ProjectContext = new ProjectContextDto
            {
                ProjectId = dashboardData.ProjectId,
                ProjectName = dashboardData.ProjectName,
                ProjectKey = dashboardData.ProjectKey,
                IncubatorId = dashboardData.IncubatorId,
                IncubatorName = dashboardData.IncubatorName,
            },
            ParticipantStats = new ParticipantStatsDto
            {
                TotalCount = dashboardData.TotalUsers,
                ActiveCount = dashboardData.ActiveUsers,
                PendingInvitations = dashboardData.PendingInvitations,
                RecentlyAdded = dashboardData.RecentUsers,
                CountByRole = dashboardData.UsersByRole,
            },
            DiagnosticStats = new DiagnosticStatsDto
            {
                TotalForms = dashboardData.TotalForms,
                CompletedForms = dashboardData.CompletedForms,
                InProgressForms = dashboardData.InProgressForms,
                NotStartedCount = dashboardData.NotStartedForms,
                CompletionRate = dashboardData.TotalForms > 0
                    ? Math.Round((double)dashboardData.CompletedForms / dashboardData.TotalForms * 100, 1)
                    : 0,
                AverageCompletionTimeHours = dashboardData.AverageCompletionHours,
            },
            PendingReviews = new PendingReviewsDto
            {
                TotalPending = dashboardData.TotalPendingReviews,
                TopReviews = dashboardData.PendingReviews.Select(pr => new PendingReviewItemDto
                {
                    Id = pr.Id,
                    ParticipantUserId = pr.UserId,
                    ParticipantName = userLookup.TryGetValue(pr.UserId, out var userInfo)
                        ? $"{userInfo.FirstName} {userInfo.LastName}"
                        : "Usuario",
                    SubmittedAt = pr.SubmittedAt,
                    DaysWaiting = pr.DaysWaiting,
                    FormType = "Diagnóstico",
                }).ToList(),
                OldestWaitingDays = dashboardData.PendingReviews.Any()
                    ? dashboardData.PendingReviews.Max(pr => pr.DaysWaiting)
                    : 0,
                AverageWaitingDays = dashboardData.PendingReviews.Any()
                    ? Math.Round(dashboardData.PendingReviews.Average(pr => pr.DaysWaiting), 1)
                    : 0,
            },
            RecentActivities = dashboardData.RecentActivities.Select(activity => new ActivityItemDto
            {
                UserId = activity.UserId,
                UserName = userLookup.TryGetValue(activity.UserId, out var userInfo)
                    ? $"{userInfo.FirstName} {userInfo.LastName}"
                    : "Usuario",
                Action = activity.Action,
                ActionDescription = GetActionDescription(activity.Action),
                Timestamp = activity.Timestamp,
                TimeAgo = GetTimeAgo(activity.Timestamp),
            }).ToList(),
            UserNames = userLookup.ToDictionary(
                kvp => kvp.Key,
                kvp => $"{kvp.Value.FirstName} {kvp.Value.LastName}"),
        };

        // Cache the result for 5 minutes
        cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        stopwatch.Stop();
        logger.LogInformation(
            "Dashboard data loaded for project {ProjectId} in {ElapsedMs}ms with {QueryCount} queries",
            request.ProjectId,
            stopwatch.ElapsedMilliseconds,
            2); // Only 2 queries: dashboard data + users batch

        return Success(result);
    }

    private static string GetActionDescription(string action)
    {
        return action switch
        {
            "form_submitted" => "Envió formulario de diagnóstico",
            "form_approved" => "Formulario aprobado",
            "user_joined" => "Se unió al proyecto",
            "invitation_sent" => "Invitación enviada",
            _ => action,
        };
    }

    private string GetTimeAgo(DateTime timestamp)
    {
        var now = timeProvider.UtcNow;
        var diff = now - timestamp;

        return diff.TotalMinutes switch
        {
            < 1 => "Hace un momento",
            < 60 => $"Hace {(int)diff.TotalMinutes} minutos",
            < 1440 => diff.TotalHours == 1 ? "Hace 1 hora" : $"Hace {(int)diff.TotalHours} horas",
            < 10080 => diff.TotalDays == 1 ? "Hace 1 día" : $"Hace {(int)diff.TotalDays} días",
            _ => timestamp.ToString("dd/MM/yyyy"),
        };
    }
}