# Work Log

## 2025-01-12 - Coordination Dashboard Performance Analysis

### Context
User reported that /Coordination/Dashboard takes an eternity to load. Performed deep performance analysis to identify root causes and created comprehensive optimization plan.

### Completed

1. **Performance Analysis**:
   - Traced entire flow from controller to database
   - Identified 20+ queries executed per page load
   - Found severe N+1 query problems in user data loading
   - Discovered duplicate project loading across handlers

2. **Root Causes Identified**:
   - `GetCoordinatorParticipantStatsQuery`: Loads project twice
   - `GetCoordinatorDiagnosticStatsQuery`: Loads project twice  
   - `GetCoordinatorPendingReviewsQuery`: N+1 for user names (up to 10 queries)
   - `GetCoordinatorRecentActivityQuery`: N+1 for user names (up to 11 queries)
   - Missing database indexes on critical columns
   - All filtering done in-memory instead of at database level

3. **Requirements Document Created**:
   - Created `REQ-011-coordination-dashboard-performance.md`
   - Saved to `.claude/requirements/pending/`
   - Comprehensive optimization plan with code examples

### Key Discoveries

**Query Explosion Pattern**:
```csharp
// PROBLEM: Each handler loads project independently
GetProjectWithUsersAsync(projectId);      // Query 1
GetProjectWithFormSubmissionsAsync(id);   // Query 2  
GetProjectWithInvitationsByExternalIdAsync(id); // Query 3
// Then N+1 for each user...
foreach (var userId in userIds) {
    await userManager.FindByIdAsync(userId); // +N queries!
}
```

**Missing Indexes**:
- No index on `ProjectFormSubmissions(ProjectId, Status)`
- No index on `ProjectUsers(ProjectId, IsActive)`
- No covering indexes for dashboard statistics

### Solution Approach

**Single Query Pattern**:
```csharp
// SOLUTION: Load all data in one optimized query
var dashboardData = await dbContext.Projects
    .Where(p => p.Id == projectId)
    .Select(p => new DashboardData {
        // Project aggregations at DB level
        TotalUsers = p.ProjectUsers.Count(u => u.IsActive),
        CompletedForms = p.FormSubmissions.Count(f => f.Status == Completed),
        // Batch load all user IDs
        AllUserIds = p.ProjectUsers.Select(u => u.UserId).ToList()
    })
    .FirstOrDefaultAsync();
```

### Files Created/Modified
- `.claude/requirements/pending/REQ-011-coordination-dashboard-performance.md`
- `.claude/CURRENT_SESSION.md` (updated with findings)

### Next Steps
1. Add database indexes (quick win - 30-50% improvement)
2. Implement batch user loading query
3. Create unified dashboard data query
4. Add request-scoped caching
5. Performance test with 100+ users