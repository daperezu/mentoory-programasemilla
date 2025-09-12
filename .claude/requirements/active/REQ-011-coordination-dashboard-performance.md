# REQ-011: Coordination Dashboard Performance Optimization

> **Priority**: P1  
> **Module**: BusinessIncubator/Core  
> **Estimate**: Large  
> **Status**: Pending  
> **Branch**: feature/dashboard-performance  

## Summary
Optimize the Coordination Dashboard (/Coordination/Dashboard) to reduce load time from 5-10 seconds to under 500ms by eliminating N+1 queries, implementing efficient data loading patterns, and adding proper database indexes.

## Business Context
The Coordination Dashboard is critically slow, taking an eternity to load due to executing 20+ database queries per page load. This impacts coordinator productivity and user experience. The dashboard executes redundant queries, loads the same data multiple times, and has severe N+1 query problems when fetching user information. Performance must be drastically improved to ensure system usability at scale.

## Acceptance Criteria
- [ ] Dashboard loads in under 500ms for projects with 100+ participants
- [ ] Reduce database queries from 20+ to maximum 3-5 per page load
- [ ] Eliminate all N+1 query patterns in dashboard data loading
- [ ] Implement request-scoped caching to prevent duplicate data loading
- [ ] Add database indexes to optimize query performance
- [ ] Maintain full DDD compliance without breaking existing functionality
- [ ] Dashboard remains fully functional with all current features intact
- [ ] Performance improvements measurable via Application Insights metrics

## Technical Requirements

### Domain Layer
**No changes required** - Performance optimization should not modify domain logic.

### Application Layer

#### New Queries to Create

1. **`GetCoordinatorDashboardCompleteDataQuery`**
   ```csharp
   public record GetCoordinatorDashboardCompleteDataQuery(
       long ProjectId,
       string CoordinatorUserId,
       DateTime? DateRangeStart = null) : IBaseRequest<CoordinatorDashboardCompleteDto>;
   ```

2. **`GetUsersByIdsQuery`** (in Auth.Application)
   ```csharp
   public record GetUsersByIdsQuery(
       IEnumerable<string> UserIds) : IBaseRequest<Dictionary<string, UserBasicInfoDto>>;
   ```

#### DTOs Required

```csharp
public class CoordinatorDashboardCompleteDto
{
    // Project Context
    public ProjectContextDto ProjectContext { get; set; }
    
    // Participant Statistics
    public ParticipantStatsDto ParticipantStats { get; set; }
    
    // Diagnostic Statistics
    public DiagnosticStatsDto DiagnosticStats { get; set; }
    
    // Pending Reviews
    public PendingReviewsDto PendingReviews { get; set; }
    
    // Recent Activity
    public List<ActivityItemDto> RecentActivities { get; set; }
    
    // User Lookup Dictionary (for avoiding N+1)
    public Dictionary<string, string> UserNames { get; set; }
}

public class ProjectContextDto
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string ProjectKey { get; set; }
    public long IncubatorId { get; set; }
    public string IncubatorName { get; set; }
}

public class ParticipantStatsDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int PendingInvitations { get; set; }
    public int RecentlyAdded { get; set; }
    public Dictionary<string, int> CountByRole { get; set; }
}

public class DiagnosticStatsDto
{
    public int TotalForms { get; set; }
    public int CompletedForms { get; set; }
    public int InProgressForms { get; set; }
    public int NotStartedCount { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTimeHours { get; set; }
}

public class PendingReviewsDto
{
    public int TotalPending { get; set; }
    public List<PendingReviewItemDto> TopReviews { get; set; } // Limited to 10
    public int OldestWaitingDays { get; set; }
    public double AverageWaitingDays { get; set; }
}
```

#### Query Handlers

**`GetCoordinatorDashboardCompleteDataQueryHandler`**
- Single database query with all necessary includes
- Use compiled query for performance
- Apply all filters at database level
- Project to DTOs using Select() for efficiency
- Batch load all required users in one query

### Infrastructure Layer

#### New Repository Methods

1. **IBusinessIncubatorRepository additions:**
```csharp
// Optimized dashboard-specific method
Task<DashboardProjectData?> GetProjectDashboardDataAsync(
    long projectId, 
    DateTime? fromDate = null,
    CancellationToken cancellationToken = default);

// Compiled query for performance
Task<ProjectStatistics> GetProjectStatisticsCompiledAsync(
    long projectId,
    CancellationToken cancellationToken = default);
```

2. **IAuthRepository additions:**
```csharp
// Batch user loading to eliminate N+1
Task<Dictionary<string, UserBasicInfo>> GetUsersByIdsAsync(
    IEnumerable<string> userIds,
    CancellationToken cancellationToken = default);
```

#### Repository Implementation Optimizations

```csharp
public async Task<DashboardProjectData?> GetProjectDashboardDataAsync(
    long projectId, 
    DateTime? fromDate = null,
    CancellationToken cancellationToken = default)
{
    var cutoffDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
    
    // Single query with all necessary data
    var query = dbContext.Projects
        .Where(p => p.Id == projectId)
        .Select(p => new DashboardProjectData
        {
            ProjectId = p.Id,
            ProjectName = p.Name,
            ProjectKey = p.Key,
            IncubatorId = p.BusinessIncubatorId,
            IncubatorName = p.BusinessIncubator.Name,
            
            // User statistics (using database aggregation)
            TotalUsers = p.ProjectUsers.Count(u => u.IsActive),
            ActiveUsers = p.ProjectUsers.Count(u => u.IsActive && u.LastActivityAt > cutoffDate),
            RecentUsers = p.ProjectUsers.Count(u => u.JoinedAt > DateTime.UtcNow.AddDays(-7)),
            UsersByRole = p.ProjectUsers
                .Where(u => u.IsActive)
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Role, x => x.Count),
            
            // Form statistics (using database aggregation)
            TotalForms = p.FormSubmissions.Count(),
            CompletedForms = p.FormSubmissions.Count(f => 
                f.Status == ProjectFormSubmissionStatus.Submitted || 
                f.Status == ProjectFormSubmissionStatus.Approved),
            InProgressForms = p.FormSubmissions.Count(f => 
                f.Status == ProjectFormSubmissionStatus.Draft),
            
            // Pending reviews (limited and filtered at DB level)
            PendingReviews = p.FormSubmissions
                .Where(f => f.Status == ProjectFormSubmissionStatus.Submitted)
                .OrderByDescending(f => f.SubmittedAt)
                .Take(10)
                .Select(f => new PendingReviewData
                {
                    Id = f.Id,
                    UserId = f.ParticipantUserId,
                    SubmittedAt = f.SubmittedAt ?? f.StartedAt,
                    DaysWaiting = EF.Functions.DateDiffDay(f.SubmittedAt ?? f.StartedAt, DateTime.UtcNow)
                })
                .ToList(),
            
            // Recent activities (limited and filtered at DB level)
            RecentSubmissions = p.FormSubmissions
                .Where(f => f.SubmittedAt > cutoffDate)
                .OrderByDescending(f => f.SubmittedAt)
                .Take(5)
                .Select(f => new ActivityData
                {
                    UserId = f.ParticipantUserId,
                    Action = "form_submitted",
                    Timestamp = f.SubmittedAt!.Value
                })
                .ToList(),
                
            // Collect all user IDs for batch loading
            AllUserIds = p.ProjectUsers.Select(u => u.UserId)
                .Union(p.FormSubmissions.Select(f => f.ParticipantUserId))
                .Distinct()
                .ToList()
        });
    
    return await query.FirstOrDefaultAsync(cancellationToken);
}
```

### Web Layer

#### Controller Optimizations

**DashboardController changes:**
```csharp
[HttpGet]
public async Task<IActionResult> Index()
{
    var stopwatch = Stopwatch.StartNew();
    
    // Get complete dashboard data in ONE query
    var dashboardData = await mediatorExecutor.SendOrThrowAsync(
        new GetCoordinatorDashboardCompleteDataQuery(
            context.ProjectId!.Value,
            CurrentUserId));
    
    // Map to view model
    var viewModel = mapper.Map<CoordinatorDashboardViewModel>(dashboardData);
    
    // Set ViewBag properties from cached data
    ViewBag.ProjectName = dashboardData.ProjectContext.ProjectName;
    ViewBag.IncubatorName = dashboardData.ProjectContext.IncubatorName;
    // ... etc
    
    stopwatch.Stop();
    logger.LogInformation("Dashboard loaded in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    
    return View(viewModel);
}

// Remove individual AJAX endpoints or make them use cached data
[HttpGet]
public IActionResult GetParticipantStats()
{
    // Return data from already loaded dashboard data
    var cachedData = HttpContext.Items["DashboardData"] as CoordinatorDashboardCompleteDto;
    return Json(new { success = true, data = cachedData?.ParticipantStats });
}
```

#### Request-Scoped Caching

```csharp
public class RequestScopedCacheService : IRequestScopedCache
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public T GetOrAdd<T>(string key, Func<Task<T>> factory) where T : class
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext.Items.TryGetValue(key, out var cached))
        {
            return (T)cached;
        }
        
        var result = factory().GetAwaiter().GetResult();
        httpContext.Items[key] = result;
        return result;
    }
}
```

## Database Changes

### New Indexes Required

```sql
-- 1. Optimize ProjectFormSubmissions queries
CREATE NONCLUSTERED INDEX IX_ProjectFormSubmissions_ProjectId_Status_SubmittedAt
ON [businessincubators].[ProjectFormSubmissions] (
    [ProjectId] ASC,
    [Status] ASC,
    [SubmittedAt] DESC
)
INCLUDE ([ParticipantUserId], [StartedAt], [ApprovedAt], [ApprovedByUserId]);

-- 2. Optimize ProjectUsers queries
CREATE NONCLUSTERED INDEX IX_ProjectUsers_ProjectId_IsActive_JoinedAt
ON [businessincubators].[ProjectUsers] (
    [ProjectId] ASC,
    [IsActive] ASC,
    [JoinedAt] DESC
)
INCLUDE ([UserId], [Role], [LastActivityAt]);

-- 3. Optimize ProjectInvitations queries
CREATE NONCLUSTERED INDEX IX_ProjectInvitations_ProjectId_Status
ON [businessincubators].[ProjectInvitations] (
    [ProjectId] ASC,
    [Status] ASC
)
INCLUDE ([CreatedAt], [Email]);

-- 4. Covering index for dashboard statistics
CREATE NONCLUSTERED INDEX IX_Projects_Dashboard_Statistics
ON [businessincubators].[Projects] ([Id])
INCLUDE ([Name], [Key], [BusinessIncubatorId], [Status]);
```

### Materialized View (Optional - Phase 2)

```sql
CREATE VIEW [businessincubators].[vw_DashboardStatistics]
WITH SCHEMABINDING
AS
SELECT 
    p.Id AS ProjectId,
    COUNT(DISTINCT pu.UserId) AS TotalUsers,
    COUNT(DISTINCT CASE WHEN pu.IsActive = 1 THEN pu.UserId END) AS ActiveUsers,
    COUNT(DISTINCT fs.Id) AS TotalForms,
    COUNT(DISTINCT CASE WHEN fs.Status IN (3,4) THEN fs.Id END) AS CompletedForms,
    COUNT_BIG(*) AS RowCount -- Required for indexed view
FROM [businessincubators].[Projects] p
LEFT JOIN [businessincubators].[ProjectUsers] pu ON p.Id = pu.ProjectId
LEFT JOIN [businessincubators].[ProjectFormSubmissions] fs ON p.Id = fs.ProjectId
GROUP BY p.Id;

-- Create unique clustered index on the view
CREATE UNIQUE CLUSTERED INDEX IX_vw_DashboardStatistics 
ON [businessincubators].[vw_DashboardStatistics] (ProjectId);
```

## UI/UX Requirements

### Progressive Loading Strategy
1. **Initial Load**: Show skeleton/placeholders immediately
2. **Critical Data**: Load project header and basic counts first
3. **Progressive Enhancement**: Load detailed widgets after initial render
4. **Loading States**: Show loading indicators for each widget

### Frontend Optimizations
```javascript
// Parallel widget loading
async function loadDashboardWidgets() {
    const widgetPromises = [
        fetch('/api/dashboard/participants'),
        fetch('/api/dashboard/diagnostics'),
        fetch('/api/dashboard/reviews'),
        fetch('/api/dashboard/activity')
    ];
    
    const results = await Promise.allSettled(widgetPromises);
    
    results.forEach((result, index) => {
        if (result.status === 'fulfilled') {
            updateWidget(index, result.value);
        } else {
            showWidgetError(index);
        }
    });
}

// Client-side caching
const DashboardCache = {
    set(key, data, ttlMinutes = 5) {
        const item = {
            data: data,
            expiry: Date.now() + (ttlMinutes * 60 * 1000)
        };
        sessionStorage.setItem(`dashboard_${key}`, JSON.stringify(item));
    },
    
    get(key) {
        const item = sessionStorage.getItem(`dashboard_${key}`);
        if (!item) return null;
        
        const parsed = JSON.parse(item);
        if (Date.now() > parsed.expiry) {
            sessionStorage.removeItem(`dashboard_${key}`);
            return null;
        }
        return parsed.data;
    }
};
```

## Dependencies
- [ ] Depends on: None (can be implemented immediately)
- [ ] Blocks: Future dashboard enhancements
- [ ] External systems: None

## Testing Requirements

### Performance Tests
```csharp
[Fact]
public async Task Dashboard_Should_Load_Under_500ms()
{
    // Arrange
    var projectId = CreateTestProjectWith100Users();
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var result = await handler.Handle(
        new GetCoordinatorDashboardCompleteDataQuery(projectId, "user-id"),
        CancellationToken.None);
    
    // Assert
    stopwatch.Stop();
    Assert.True(stopwatch.ElapsedMilliseconds < 500);
    Assert.NotNull(result.Value);
}

[Fact]
public async Task Dashboard_Should_Execute_Maximum_3_Queries()
{
    // Use SQL profiler or EF Core logging to verify query count
    var queryCount = 0;
    dbContext.Database.Log = sql => queryCount++;
    
    // Act
    await handler.Handle(query, CancellationToken.None);
    
    // Assert
    Assert.True(queryCount <= 3, $"Expected max 3 queries but executed {queryCount}");
}
```

### Load Tests
- Test with 1, 10, 100, 1000 users in project
- Test with 0, 100, 1000, 10000 form submissions
- Verify sub-second response times at all scales

## Security Considerations
- **Authentication**: Maintain existing authentication requirements
- **Authorization**: Verify coordinator access before loading data
- **Data Protection**: No sensitive data in client-side cache
- **Query Injection**: Use parameterized queries only

## Documentation Updates
- [ ] Update `.claude/architecture.md` with dashboard optimization patterns
- [ ] Document request-scoped caching pattern in `.claude/web-patterns.md`
- [ ] Add performance optimization guide to `.claude/common-issues.md`
- [ ] Update API documentation with new endpoints

## Implementation Notes

### Critical Performance Patterns

1. **Single Database Round-Trip**
   - Load ALL dashboard data in one query
   - Use includes and projections efficiently
   - Apply filters at database level, not in memory

2. **Batch User Loading**
   - Collect all user IDs from all widgets
   - Load users once with `GetUsersByIdsAsync`
   - Pass user dictionary to all components

3. **Database-Level Aggregation**
   - Use `Count()`, `Sum()`, `GroupBy()` in LINQ queries
   - Let SQL Server do the aggregation
   - Avoid loading collections just to count them

4. **Compiled Queries**
   ```csharp
   private static readonly Func<BusinessIncubatorDbContext, long, Task<ProjectStats>> 
       GetProjectStatsCompiled = EF.CompileAsyncQuery(
           (BusinessIncubatorDbContext context, long projectId) =>
               context.Projects
                   .Where(p => p.Id == projectId)
                   .Select(p => new ProjectStats { /* ... */ })
                   .FirstOrDefault());
   ```

### Migration Strategy

1. **Phase 1**: Implement optimized query handler (keep old endpoints)
2. **Phase 2**: Update controller to use new handler
3. **Phase 3**: Deprecate individual widget endpoints
4. **Phase 4**: Remove old code after verification

## Definition of Done
- [ ] All query handlers implemented with single database query pattern
- [ ] Database indexes created and query plans verified
- [ ] N+1 queries completely eliminated
- [ ] Dashboard loads in under 500ms for 100+ user projects
- [ ] Request-scoped caching implemented
- [ ] All existing functionality preserved
- [ ] Performance metrics logged to Application Insights
- [ ] Load tests pass at all scales
- [ ] StyleCop compliance verified (zero warnings)
- [ ] Documentation updated with patterns
- [ ] Code reviewed by senior developer
- [ ] Deployed to staging and performance verified

## Follow-up Tasks
- **Phase 2**: Implement materialized views for instant loading
- **Phase 3**: Add Redis distributed caching for multi-server deployment
- **Phase 4**: Real-time updates using SignalR
- **Phase 5**: Predictive pre-loading based on user behavior
- **Enhancement**: Dashboard customization per coordinator preferences
- **Enhancement**: Export dashboard data to Excel/PDF
- **Enhancement**: Historical trend analysis charts

---

**Performance Impact Summary:**
- **Current State**: 20+ queries, 5-10 seconds load time, poor user experience
- **Target State**: 2-3 queries, <500ms load time, instant user feedback
- **Improvement**: 90% reduction in queries, 10-20x performance improvement

**Risk Assessment:**
- **Low Risk**: New queries don't modify existing functionality
- **Testing Required**: Extensive load testing before production
- **Rollback Plan**: Keep old endpoints temporarily for quick rollback