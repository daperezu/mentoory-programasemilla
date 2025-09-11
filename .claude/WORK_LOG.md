# Work Log

## 2025-01-10 - FormReview 403 Error & Query Logic Fixes

### Context
Fixed critical issues preventing FormReview page from displaying submissions. User reported 403 error and no data showing despite having submissions in database.

### Completed

1. **Database Deployment Script**:
   - Created `Db/Publish-LinaDb.ps1` with automatic tool discovery
   - Added MSBuild/SqlPackage path resolution for multiple VS versions
   - Created `LinaDb.Development.publish.xml` for LocalDB deployment

2. **WebFeatures Permission Fix**:
   - Added missing `'Coordination.FormReview.GetAllSubmissions.Post'` to seed data
   - File: `Db/PostDeployment/001.SeedWebFeatures.sql` (line 594)

3. **Query Logic Bug Fix**:
   - **Problem**: Query checked `p.ProjectUsers.Any(...)` but Project entity has no ProjectUsers navigation
   - **Solution**: Simplified to use GetProjectsByUserAsync results directly
   - File: `BusinessIncubator.Application/Reviews/Queries/GetAllSubmissionsForReview/GetAllSubmissionsForReviewQuery.cs`

### Key Findings

**Root Cause Analysis**:
1. Permission was missing from database seed → 403 error
2. Query logic tried to access non-existent navigation property → no results
3. GetProjectsByUserAsync already filters accessible projects, no need to re-check

**Code Fix Applied**:
```csharp
// Before: Checking non-existent property
var hasAccess = userProjects.Any(p => p.Id == request.ProjectId.Value &&
    p.ProjectUsers.Any(pu => pu.UserId == request.UserId && ...));

// After: Simplified check
var hasAccess = userProjects.Any(p => p.Id == request.ProjectId.Value);
```

### Files Modified
- `Db/PostDeployment/001.SeedWebFeatures.sql`
- `Db/Publish-LinaDb.ps1` (new)
- `Db/LinaDb.Development.publish.xml` (new)
- `Web/Areas/Coordination/Controllers/FormReviewController.cs`
- `BusinessIncubator.Application/Reviews/Queries/GetAllSubmissionsForReview/GetAllSubmissionsForReviewQuery.cs`

### Next Steps
1. Run `.\Publish-LinaDb.ps1 -Publish` to deploy database changes
2. Restart application
3. Test FormReview page shows submissions correctly