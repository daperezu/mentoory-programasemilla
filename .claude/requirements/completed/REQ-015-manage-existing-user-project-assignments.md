# REQ-015: Manage Project Assignments for Existing Users

## Requirement ID
REQ-015

## Status
✅ COMPLETED

## Priority
MEDIUM

## Created Date
2025-01-30

## Completed Date
2025-10-16

## Completion Summary
Successfully implemented comprehensive project assignment management for existing users. All 10 stages completed with clean build (0 errors, 0 warnings).

**Key Achievements**:
- 5 controller actions for full CRUD operations on project assignments
- Cross-domain orchestration query joining Auth + BusinessIncubator domains
- DataTable UI with modals, cascading dropdowns, and AJAX operations
- Role-based security via ASP.NET Core Authorization attributes
- Soft delete pattern for audit trail preservation

**Implementation Variance**:
- Security implemented via `[Authorize(Roles = "...")]` instead of WebFeatures SQL (system doesn't use custom WebFeatures table)
- Query placed in Orchestration.Application instead of Auth.Application (cross-domain pattern)

**Build Issues Resolved**:
- File corruption from previous session
- Tuple access pattern errors (CS0023)
- Missing namespace references (CS0234)
- StyleCop SA1202 member ordering

**Files Created**: 9 new files (queries, commands, views, viewmodels, JavaScript)
**Files Modified**: 2 files (controller, details view)

## Business Context
LinaSys currently provides two ways to assign users to projects:
1. During initial user creation via the UserManagement/Create wizard
2. Via bulk CSV/Excel import through ParticipantController.BulkInvite

However, there is **no user interface to manage project assignments for existing users**. Once a user is created, coordinators and administrators cannot:
- Add the user to additional projects
- Remove the user from projects
- Change the user's role within a project
- View all projects the user has access to in one place

This creates significant workflow friction, forcing administrators to either:
- Delete and recreate users to change project assignments
- Manually update database records (unacceptable)
- Use bulk import for single-user changes (inefficient)

## Current Problems
1. **No visibility**: User details page doesn't show which projects the user has access to
2. **No add functionality**: Cannot assign an existing user to a new project through the UI
3. **No remove functionality**: Cannot remove a user from a project through the UI
4. **No role modification**: Cannot change a user's role within a specific project
5. **Workflow inefficiency**: Coordinators must recreate users or use workarounds
6. **Poor UX**: Administrators expect this basic functionality to exist

## Proposed Solution
Add a comprehensive "Manage Project Access" feature accessible from the User Details page that provides:

1. **View Current Assignments**: DataTable showing all projects user has access to
2. **Add to Projects**: Modal interface to assign user to additional projects
3. **Remove from Projects**: Deactivate user access to specific projects
4. **Modify Roles**: Change user's role within a project
5. **Audit Trail**: Track who made changes and when

## Requirements

### Functional Requirements
1. **Display Current Project Assignments**
   - Show all projects the user is assigned to (active and inactive)
   - Display: Project Name, Incubator, Role, Status, Join Date, Actions
   - Support filtering and searching
   - Indicate active vs inactive assignments

2. **Add User to New Projects**
   - Button: "Add to Project"
   - Modal with cascading dropdowns:
     - Select Incubator (filtered by user's data scope)
     - Select Project (filtered by selected incubator)
     - Select Role (based on current user's permissions)
   - Validate user doesn't already have active access
   - Use existing `AssignUserToProjectOrchestrationCommand`

3. **Remove User from Projects**
   - "Remove" action button per project row
   - Confirmation dialog: "¿Está seguro que desea remover a [User] del proyecto [Project]?"
   - Deactivate access (soft delete pattern)
   - Record who performed the removal and when

4. **Reactivate Project Access**
   - For inactive assignments, show "Reactivate" button
   - Restore access with same role
   - Update LastSyncedAt timestamp

5. **Change Project Role**
   - "Change Role" action button per project row
   - Modal showing current role and dropdown for new role
   - Validate new role is within current user's permissions
   - Update via `AssignUserToProjectOrchestrationCommand`

6. **Access Control**
   - Global Administrators: Can manage all project assignments
   - Administrators: Can manage assignments within their incubator
   - Coordinators: Can manage assignments within their projects
   - Other roles: Read-only view of their own assignments

### Non-Functional Requirements
1. **Performance**: Page load <2 seconds with up to 50 project assignments
2. **Consistency**: Use existing orchestration commands (no new business logic)
3. **Spanish UI**: All text in Spanish per LinaSys standards
4. **Clean Build**: Zero warnings per StyleCop rules
5. **Security**: Update `001.SeedWebFeatures.sql` with new actions
6. **Audit**: Log all changes to UserActivities table

## Technical Design

### Architecture

```
Web Layer (MVC)
├── Controller: UserManagementController
│   ├── ManageProjects(userId) - GET: Main page
│   ├── ListUserProjects(userId) - POST: DataTable data
│   ├── AddToProject(model) - POST: Assign to new project
│   ├── RemoveFromProject(userId, projectId) - POST: Deactivate access
│   └── ChangeProjectRole(model) - POST: Update role
│
├── Views
│   └── UserManagement/
│       └── ManageProjects.cshtml - Main management page
│
└── ViewModels
    ├── ManageProjectsViewModel - Page model
    ├── UserProjectAssignmentDto - Row data
    ├── AddToProjectViewModel - Add modal
    └── ChangeRoleViewModel - Change role modal

Application Layer
├── Queries
│   └── GetUserProjectAssignmentsQuery - Fetch all assignments
│       └── Returns: List<UserProjectAssignmentDto>
│
└── Commands (Reuse existing)
    ├── AssignUserToProjectOrchestrationCommand - Add/Update
    └── Auth.Application.Commands.DeactivateProjectAccessCommand - Remove

Domain Layer (No changes - reuse existing)
├── UserProjectAccess entity (Auth.Domain)
└── ProjectUser entity (BusinessIncubator.Domain)
```

### File Structure
```
Web/Areas/Coordination/
├── Controllers/
│   └── UserManagementController.cs                (modify - add 5 actions)
├── Views/UserManagement/
│   ├── Details.cshtml                              (modify - add button)
│   └── ManageProjects.cshtml                       (new - main page)
└── Models/UserManagement/
    ├── ManageProjectsViewModel.cs                  (new)
    ├── UserProjectAssignmentDto.cs                 (new)
    ├── AddToProjectViewModel.cs                    (new)
    └── ChangeProjectRoleViewModel.cs               (new)

Auth.Application/
└── Queries/
    ├── GetUserProjectAssignmentsQuery.cs           (new)
    └── GetUserProjectAssignmentsQueryHandler.cs    (new)

Auth.Application/Commands/
└── DeactivateProjectAccessCommand.cs               (new - if doesn't exist)

Web/wwwroot/js/coordination/
└── user-manage-projects.js                         (new - UI interactions)

PostDeployment/
└── 001.SeedWebFeatures.sql                         (modify - add actions)
```

### Data Flow

#### Get User Projects
```
User → ManageProjects(userId) → GetUserProjectAssignmentsQuery
    → Auth.Repository.GetUserProjectAccessesAsync(userId)
    → Join with BusinessIncubator.Projects for details
    → Return DataTable with assignments
```

#### Add to Project
```
User → Modal: Select Incubator/Project/Role
    → AddToProject(model)
    → Validate user doesn't have active access
    → AssignUserToProjectOrchestrationCommand
    → Refresh DataTable
```

#### Remove from Project
```
User → Click "Remove" → Confirmation
    → RemoveFromProject(userId, projectId)
    → DeactivateProjectAccessCommand
    → Set IsActive = false, DeactivatedAt = now
    → Refresh DataTable
```

## Implementation Tasks

### Stage 1: Query Infrastructure (3 hours)
- [ ] Create `GetUserProjectAssignmentsQuery` in Auth.Application
- [ ] Create `UserProjectAssignmentDto` with joined data
- [ ] Implement query handler with EF joins
- [ ] Add repository method: `GetUserProjectAccessesWithDetailsAsync`
- [ ] Unit test query handler

### Stage 2: Controller Actions (4 hours)
- [ ] Add `ManageProjects(userId)` GET action
- [ ] Add `ListUserProjects(userId)` POST for DataTable
- [ ] Add `AddToProject(model)` POST action
- [ ] Add `RemoveFromProject(userId, projectId)` POST action
- [ ] Add `ChangeProjectRole(model)` POST action
- [ ] Add access control checks per role
- [ ] Add error handling and toast notifications

### Stage 3: ViewModels (2 hours)
- [ ] Create `ManageProjectsViewModel`
- [ ] Create `UserProjectAssignmentDto`
- [ ] Create `AddToProjectViewModel` with validation
- [ ] Create `ChangeProjectRoleViewModel`
- [ ] Add Spanish display names and validation messages

### Stage 4: Main View (3 hours)
- [ ] Create `ManageProjects.cshtml` with layout
- [ ] Add DataTable with columns: Project, Incubator, Role, Status, Join Date
- [ ] Add action buttons: Add, Remove, Reactivate, Change Role
- [ ] Add status badges (Active/Inactive)
- [ ] Add responsive design for mobile

### Stage 5: Modals (3 hours)
- [ ] Create "Add to Project" modal
- [ ] Add cascading dropdowns (Incubator → Project)
- [ ] Create "Change Role" modal with role dropdown
- [ ] Add confirmation dialog for remove action
- [ ] Implement client-side validation

### Stage 6: JavaScript (4 hours)
- [ ] Create `user-manage-projects.js`
- [ ] Initialize DataTable with server-side processing
- [ ] Implement cascading dropdown logic
- [ ] Add modal handlers (show/hide/submit)
- [ ] Implement AJAX calls for add/remove/change
- [ ] Add success/error toast notifications
- [ ] Handle DataTable refresh after operations

### Stage 7: Commands (if needed) (2 hours)
- [ ] Check if `DeactivateProjectAccessCommand` exists
- [ ] Create if missing, with validation
- [ ] Add integration event publishing
- [ ] Unit test command handler

### Stage 8: Integration (2 hours)
- [ ] Add "Manage Projects" button to Details.cshtml
- [ ] Update navigation to ManageProjects page
- [ ] Test cascading dropdowns
- [ ] Test add/remove/change operations
- [ ] Verify access control per role

### Stage 9: Security & Permissions (2 hours)
- [ ] Update `001.SeedWebFeatures.sql`:
  - UserManagement.ManageProjects.Page
  - UserManagement.ManageProjects.Get
  - UserManagement.AddToProject.Post
  - UserManagement.RemoveFromProject.Post
  - UserManagement.ChangeProjectRole.Post
- [ ] Assign permissions to appropriate roles
- [ ] Test with different user roles

### Stage 10: Testing & Documentation (3 hours)
- [ ] Test as Global Administrator
- [ ] Test as Administrator (scoped to incubator)
- [ ] Test as Coordinator (scoped to project)
- [ ] Test error scenarios (already assigned, no permission)
- [ ] Update CURRENT_SESSION.md
- [ ] Update architecture.md if needed

## Success Criteria
1. **Visibility**: User Details page shows link to "Manage Projects"
2. **View Assignments**: ManageProjects page displays all user's project assignments
3. **Add Functionality**: Can assign existing user to new project with role selection
4. **Remove Functionality**: Can deactivate user's access to a project
5. **Reactivate Functionality**: Can restore inactive project access
6. **Role Change**: Can modify user's role within a project
7. **Access Control**: Each role sees only projects within their scope
8. **Performance**: Page loads in <2 seconds with 50 assignments
9. **Security**: All actions properly secured in WebFeatures
10. **Clean Build**: Zero errors, zero warnings

## Risk Analysis

### Risks
| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Duplicate assignments | Medium | Medium | Check for active access before adding |
| Permission bypass | Low | High | Thorough access control testing |
| Data inconsistency | Low | High | Use existing orchestration commands |
| Performance with many projects | Low | Medium | Pagination, server-side processing |
| UI complexity | Medium | Low | Clear user feedback, confirmation dialogs |

### Mitigation Strategies
1. **Validation**: Always check for existing active assignments before creating
2. **Reuse Commands**: Use existing `AssignUserToProjectOrchestrationCommand` to maintain consistency
3. **Soft Deletes**: Deactivate instead of hard delete to preserve audit trail
4. **Access Checks**: Verify current user has permission for target incubator/project
5. **Integration Events**: Ensure all changes publish appropriate events
6. **Atomic Operations**: Each change in its own transaction for rollback capability

## Dependencies
- Existing `AssignUserToProjectOrchestrationCommand` (already exists)
- Existing `GetProjectsByIncubatorQuery` (already exists)
- Existing `GetAllIncubatorsQuery` (already exists)
- Auth.Domain `UserProjectAccess` entity (already exists)
- BusinessIncubator.Domain `ProjectUser` entity (already exists)
- May need new `DeactivateProjectAccessCommand` (check if exists)

## Estimated Timeline
- **Development Effort**: 28 hours
- **Testing & Validation**: 6 hours
- **Documentation**: 2 hours
- **Total Effort**: 36 hours
- **Calendar Time**: 1 week with serial work
- **Deployment Window**: No downtime required (additive feature)

## Testing Plan

### Unit Testing
- Test GetUserProjectAssignmentsQuery with various user scenarios
- Test access control: GlobalAdmin, Admin, Coordinator, Starter
- Test validation: duplicate assignments, invalid roles
- Test DeactivateProjectAccessCommand if new

### Integration Testing
- Test full add workflow: select incubator → project → role → submit
- Test remove with confirmation
- Test reactivate workflow
- Test role change workflow
- Verify integration events are published
- Test cascading dropdowns behavior

### User Acceptance Testing
- Verify GlobalAdmin can manage all users
- Verify Admin can manage users in their incubator only
- Verify Coordinator can manage users in their projects only
- Verify Starters cannot access management page
- Test error messages in Spanish
- Test with 50+ project assignments for performance

## Rollback Plan

### Trigger Conditions
- Security vulnerabilities discovered
- Data corruption or inconsistency
- Performance degradation
- Critical bugs in production

### Rollback Procedure
1. Remove navigation button from Details.cshtml
2. Remove WebFeatures entries (revert `001.SeedWebFeatures.sql`)
3. Keep database schema (no schema changes, only data)
4. Keep new queries/commands (harmless if unused)
5. Redeploy previous version of affected files
6. No data loss (all changes are additive)

## Communication Plan
1. **Pre-implementation**: Review with coordinators for UX feedback
2. **During development**: Daily standup updates
3. **Pre-deployment**: Training session for administrators
4. **Post-deployment**: User guide in Spanish
5. **Support**: Monitor for first week, gather feedback

## Notes
- This feature completes the user management workflow
- Reuses existing domain logic (no new business rules)
- Purely additive (no breaking changes)
- Follows existing LinaSys patterns (DataTable, modals, toasts)
- All UI text must be in Spanish
- Maintains clean architecture (controllers use MediatorExecutor only)
- Uses existing orchestration commands for consistency

## Related Requirements
- REQ-001: User Creation with Role Access (completed) - This extends that functionality
- REQ-013: Registration Email Refactoring (completed) - May need email notification on assignment

## Open Questions
1. Should we send email notifications when users are added/removed from projects?
2. Should inactive assignments be visible by default or require a filter?
3. Do we need bulk operations (select multiple users, assign to project)?
4. Should we show assignment history or only current state?

## References
- LinaSys Architecture Guide: `.claude/architecture.md`
- LinaSys Web Patterns: `.claude/web-patterns.md`
- LinaSys DDD Patterns: `.claude/ddd-patterns.md`
- Existing UserManagementController: `Web/Areas/Coordination/Controllers/UserManagementController.cs`
- AssignUserToProjectOrchestrationCommand: `Orchestration.Application/UserManagement/Commands/AssignUserToProjectOrchestrationCommand.cs`
