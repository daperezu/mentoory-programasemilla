# REQ-016 Implementation Specification

**Requirement**: Automatic Project Form Submission Creation
**Implementation Date**: 2025-10-22
**Status**: Planning Complete → Ready for Implementation

## Implementation Checklist

### Pre-Implementation
- [x] Requirement document created (REQ-016-auto-form-submission-creation.md)
- [x] Current session archived to WORK_LOG.md
- [x] Todo list cleared
- [x] This implementation spec created
- [ ] Code analysis complete
- [ ] Implementation ready to start

### Implementation Tasks
- [ ] Create UserAddedToProjectHandler.cs
- [ ] Verify MediatR registration
- [ ] Build and fix any errors
- [ ] Test with manual user assignment
- [ ] Test with bulk invite
- [ ] Test idempotency
- [ ] Verify logs

### Post-Implementation
- [ ] Clean build (0 errors, 0 warnings)
- [ ] Update CURRENT_SESSION.md
- [ ] Archive to WORK_LOG.md
- [ ] Move requirement to completed/

## File Structure

### New Files (1 file)

```
BusinessIncubator.Application/
  IntegrationEventHandlers/
    UserAddedToProjectHandler.cs       [NEW] - Main event handler
```

### Modified Files (1 file, optional verification only)

```
BusinessIncubator.Application/
  DependencyInjection.cs                [VERIFY] - MediatR assembly scanning
```

## Code Implementation Details

### File: UserAddedToProjectHandler.cs

**Location**: `BusinessIncubator.Application/IntegrationEventHandlers/UserAddedToProjectHandler.cs`

**Namespace**: `LinaSys.BusinessIncubator.Application.IntegrationEventHandlers`

**Using Statements**:
```csharp
using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;
```

**Class Structure**:
```csharp
/// <summary>
/// Handles the UserAddedToProject integration event to automatically create
/// ProjectFormSubmission records for active form collection stages.
/// </summary>
public sealed class UserAddedToProjectHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<UserAddedToProjectHandler> logger)
    : INotificationHandler<UserAddedToProjectIntegrationEvent>
{
    public async Task Handle(
        UserAddedToProjectIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Implementation here
    }
}
```

### Implementation Logic Flow

#### Step 1: Validate Role (Lines ~35-45)
```csharp
// Only create forms for roles that need to fill forms
var rolesRequiringForms = new[] { Roles.Starter }; // Can add more roles later
if (!rolesRequiringForms.Contains(notification.Role))
{
    logger.LogDebug(
        "Role {Role} does not require automatic form creation for user {UserId}",
        notification.Role,
        notification.UserId);
    return;
}
```

#### Step 2: Fetch Project with Dependencies (Lines ~47-60)
```csharp
// Get project with stages
var project = await repository.GetProjectWithStagesByIdAsync(
    notification.ProjectId,
    cancellationToken);

if (project is null)
{
    logger.LogWarning(
        "Project {ProjectId} not found when processing UserAddedToProject for user {UserId}",
        notification.ProjectId,
        notification.UserId);
    return; // Graceful degradation - don't fail user assignment
}
```

#### Step 3: Fetch Knowledge Structure (Lines ~62-75)
```csharp
// Get knowledge structure for schema version
var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(
    project.Id,
    cancellationToken);

if (knowledgeStructure is null)
{
    logger.LogWarning(
        "Project {ProjectId} has no knowledge structure. Cannot create forms for user {UserId}",
        notification.ProjectId,
        notification.UserId);
    return; // Can't create forms without structure
}
```

#### Step 4: Find Active Form Collection Stages (Lines ~77-95)
```csharp
var currentDate = timeProvider.UtcNow;

// Find all active form collection stages within their time window
var activeFormStages = project.ProjectStages
    .Where(stage =>
        stage.IsActive &&
        (stage.Type == ProjectStageType.InitialFormCollection ||
         stage.Type == ProjectStageType.FinalFormCollection) &&
        stage.IsWithinPeriod(currentDate))
    .ToList();

if (!activeFormStages.Any())
{
    logger.LogInformation(
        "No active form collection stages for project {ProjectId}. No forms created for user {UserId}",
        notification.ProjectId,
        notification.UserId);
    return; // Normal case - no active form stages
}
```

#### Step 5: Create Form Submissions (Lines ~97-160)
```csharp
var formsCreated = 0;
var formsAlreadyExisted = 0;

foreach (var stage in activeFormStages)
{
    try
    {
        // Determine phase from stage type
        var phase = ProjectFormSubmission.GetPhaseForStage(stage.Type);

        if (phase == QuestionPhase.Undefined)
        {
            logger.LogWarning(
                "Stage {StageId} has type {StageType} which doesn't map to a form phase. Skipping.",
                stage.Id,
                stage.Type);
            continue;
        }

        // Check if submission already exists (idempotency)
        var existingSubmission = await repository.GetFormSubmissionAsync(
            project.Id,
            notification.UserId,
            phase,
            cancellationToken);

        if (existingSubmission is not null)
        {
            logger.LogDebug(
                "Form submission already exists for user {UserId}, project {ProjectId}, phase {Phase}. Skipping creation.",
                notification.UserId,
                notification.ProjectId,
                phase);
            formsAlreadyExisted++;
            continue; // Idempotency - don't create duplicate
        }

        // Create new form submission
        var submission = ProjectFormSubmission.CreateForPhase(
            projectId: project.Id,
            participantUserId: notification.UserId,
            formSchemaVersion: knowledgeStructure.CurrentVersion,
            phase: phase,
            projectStageId: stage.Id,
            startedAt: currentDate);

        repository.AddFormSubmission(submission);
        formsCreated++;

        logger.LogInformation(
            "Created {Phase} form submission for user {UserId} in project {ProjectId} (stage {StageId})",
            phase,
            notification.UserId,
            notification.ProjectId,
            stage.Id);
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Error creating form submission for user {UserId}, project {ProjectId}, stage {StageId}",
            notification.UserId,
            notification.ProjectId,
            stage.Id);
        // Continue processing other stages
    }
}
```

#### Step 6: Save Changes (Lines ~162-180)
```csharp
if (formsCreated > 0)
{
    try
    {
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Successfully created {Count} form submission(s) for user {UserId} in project {ProjectId}. {ExistingCount} already existed.",
            formsCreated,
            notification.UserId,
            notification.ProjectId,
            formsAlreadyExisted);
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Failed to save form submissions for user {UserId} in project {ProjectId}",
            notification.UserId,
            notification.ProjectId);
        // Don't throw - graceful degradation, GetOrCreateFormSubmissionCommand serves as fallback
    }
}
```

## Repository Methods Used

All methods already exist - no new repository methods needed:

| Method | Purpose | Return Type |
|--------|---------|-------------|
| `GetProjectWithStagesByIdAsync(long, CT)` | Fetch project + stages | `Task<Project?>` |
| `GetProjectKnowledgeStructureAsync(long, CT)` | Get form schema version | `Task<ProjectKnowledgeStructure?>` |
| `GetFormSubmissionAsync(long, string, QuestionPhase, CT)` | Check existing (idempotency) | `Task<ProjectFormSubmission?>` |
| `AddFormSubmission(ProjectFormSubmission)` | Add new submission | `void` |
| `UnitOfWork.SaveChangesAsync(CT)` | Persist changes | `Task<int>` |

## Integration Event

**Event**: `UserAddedToProjectIntegrationEvent`
**Namespace**: `LinaSys.Shared.Application.IntegrationEvents.Auth`

**Event Properties**:
```csharp
public record UserAddedToProjectIntegrationEvent(
    string UserId,           // User ID being assigned
    string UserEmail,        // User email
    string UserName,         // Full name
    long ProjectId,          // Internal project ID
    string ProjectName,      // Project display name
    long IncubatorId,        // Incubator ID
    string Role,             // Role being assigned (Starter, Coordinator, etc.)
    DateTime OccurredAt)     // Event timestamp
    : INotification;
```

**Published By**:
1. `BulkInviteParticipantsCommand:690-701` - After adding user to project
2. `AssignUserToProjectOrchestrationCommand` - (implied, needs verification)
3. Integration event handlers for invitation acceptance - (implied, needs verification)

## MediatR Registration

**File**: `BusinessIncubator.Application/DependencyInjection.cs`

**Expected Code** (around line 20-25):
```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
});
```

This assembly scanning automatically discovers `INotificationHandler<T>` implementations.
**Action**: Verify only, no changes needed (MediatR handles registration automatically).

## Testing Strategy

### Manual Testing Scenarios

#### Test 1: New User Assignment
```
1. Login as coordinator
2. Navigate to Users > Manage
3. Create new user with Starter role
4. Assign to project with active InitialFormCollection stage
5. Expected: Form appears immediately on user's dashboard
6. Verify: Check logs for "Created Start form submission" message
```

#### Test 2: Bulk Invite
```
1. Login as coordinator
2. Navigate to project > Bulk Invite
3. Upload CSV with 3 users
4. Expected: All 3 users get form submissions created
5. Verify: Check database for 3 new ProjectFormSubmissions records
6. Verify: Logs show "Created 3 form submission(s)"
```

#### Test 3: Idempotency
```
1. Assign user to project (creates form)
2. Assign same user to same project again
3. Expected: No duplicate form created
4. Verify: Log shows "Form submission already exists. Skipping creation."
5. Verify: Only 1 ProjectFormSubmissions record in database
```

#### Test 4: No Active Stage
```
1. Create project with no active stages (or all stages inactive)
2. Assign user to project
3. Expected: No forms created, no errors
4. Verify: Log shows "No active form collection stages"
```

#### Test 5: Multiple Active Stages
```
1. Create project with both InitialFormCollection and FinalFormCollection active
2. Assign user to project
3. Expected: 2 form submissions created (Start + Final)
4. Verify: 2 ProjectFormSubmissions records with different phases
```

### Database Verification Queries

**Check form submissions created**:
```sql
SELECT
    pfs.ExternalId,
    pfs.ParticipantUserId,
    pfs.Phase,
    pfs.Status,
    pfs.StartedAt,
    ps.Title as StageName,
    ps.Type as StageType
FROM [businessincubators].[ProjectFormSubmissions] pfs
INNER JOIN [businessincubators].[ProjectStages] ps ON pfs.ProjectStageId = ps.Id
WHERE pfs.ProjectId = @ProjectId
AND pfs.ParticipantUserId = @UserId
ORDER BY pfs.StartedAt DESC;
```

**Check for duplicates (should return 0)**:
```sql
SELECT
    ProjectId,
    ParticipantUserId,
    Phase,
    COUNT(*) as DuplicateCount
FROM [businessincubators].[ProjectFormSubmissions]
GROUP BY ProjectId, ParticipantUserId, Phase
HAVING COUNT(*) > 1;
```

### Log Verification

**Expected log entries** (INFO level):
```
Created Start form submission for user {userId} in project {projectId} (stage {stageId})
Successfully created 1 form submission(s) for user {userId} in project {projectId}. 0 already existed.
```

**Expected log entries** (DEBUG level):
```
Form submission already exists for user {userId}, project {projectId}, phase Start. Skipping creation.
```

**Expected log entries** (WARNING level):
```
Project {projectId} not found when processing UserAddedToProject for user {userId}
Project {projectId} has no knowledge structure. Cannot create forms for user {userId}
```

## Common Issues & Solutions

### Issue 1: Handler Not Executing
**Symptom**: User assigned but no logs from handler
**Diagnosis**: Check if integration event is published
**Solution**: Verify `UserAddedToProjectIntegrationEvent` is published after user assignment

### Issue 2: Forms Created But Empty
**Symptom**: Forms created but TotalQuestions = 0
**Solution**: Normal - questions are counted when form is first loaded, not at creation

### Issue 3: Duplicate Forms
**Symptom**: Multiple submissions for same user/project/phase
**Diagnosis**: Idempotency check failing
**Solution**: Verify `GetFormSubmissionAsync` query is working correctly

### Issue 4: No Forms for Valid Stage
**Symptom**: Active stage but no forms created
**Diagnosis**: Stage might be outside time window
**Solution**: Check stage StartDate/EndDate vs current date

## Rollback Procedure

If critical issues arise:

1. **Quick disable**: Comment out handler registration in DependencyInjection.cs
2. **Remove handler**: Delete UserAddedToProjectHandler.cs
3. **Build and deploy**: System falls back to lazy creation via GetOrCreateFormSubmissionCommand
4. **No data cleanup needed**: Existing submissions remain valid

## Documentation Updates

### Update CURRENT_SESSION.md
```markdown
## 🎯 Current Status: REQ-016 Implementation in Progress
**Branch**: develop
**Build**: In Progress
**Session Date**: 2025-10-22

### Current Task
- REQ-016: Automatic Project Form Submission Creation
- Creating integration event handler for proactive form creation
- Target: Users see forms immediately after assignment

### Implementation Status
- [x] Requirement documented
- [x] Implementation spec created
- [ ] Handler implementation
- [ ] Testing
- [ ] Build verification
```

### Update WORK_LOG.md (after completion)
```markdown
## 2025-10-22 - REQ-016 Complete: Automatic Form Submission Creation

### Problem Solved
Users didn't see forms on dashboard until they navigated to form editor URL.
Lazy creation caused confusion and inconsistent user experience.

### Implementation Completed
1. Created `UserAddedToProjectHandler.cs` integration event handler
2. Listens to `UserAddedToProjectIntegrationEvent` (published by 3 assignment flows)
3. Automatically creates ProjectFormSubmission for active form collection stages
4. Implements idempotency checks to prevent duplicates
5. Graceful error handling - failures don't block user assignment

### Files Created
- BusinessIncubator.Application/IntegrationEventHandlers/UserAddedToProjectHandler.cs

### Build Status
✅ 0 errors, 0 warnings

### Testing Results
- ✅ New user assignment → form appears immediately
- ✅ Bulk invite → all users get forms
- ✅ Idempotency → no duplicates on re-assignment
- ✅ No active stage → no forms, no errors
- ✅ Multiple stages → multiple forms created

### Key Patterns Used
- Integration event handler pattern (ADR-001)
- Graceful degradation (failures don't block assignments)
- Idempotency checks before creation
- Comprehensive logging for troubleshooting
```

## Success Criteria

**Implementation Complete When**:
- [x] Requirement document created and moved to active/
- [x] Implementation spec created (this document)
- [ ] UserAddedToProjectHandler.cs created and compiles
- [ ] All 5 manual test scenarios pass
- [ ] Build: 0 errors, 0 warnings
- [ ] Logs show expected messages
- [ ] Database queries confirm correct behavior
- [ ] Documentation updated

**Ready for Production When**:
- All success criteria met
- Code reviewed (if applicable)
- Tested with real project/user data
- Monitoring confirms no performance issues
- Support team briefed on new behavior

---

**Implementation Notes**:
- Use `sealed class` for handler (performance optimization)
- Use primary constructor syntax (C# 12 feature)
- Follow existing handler patterns (see `ProjectInvitationAcceptedHandler.cs`)
- Comprehensive logging at all decision points
- No exceptions thrown - all errors logged and swallowed
