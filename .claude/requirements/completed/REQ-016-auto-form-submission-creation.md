# REQ-016: Automatic Project Form Submission Creation

**Status**: Active
**Priority**: High
**Started**: 2025-10-22
**Assigned**: Claude Code
**Type**: Enhancement

## Problem Statement

When users are assigned to projects (either newly created or existing users), they do not automatically see pending forms on their dashboard. This occurs because ProjectFormSubmission records are created lazily (on-demand) when users navigate to the form page, rather than proactively when users are assigned to projects.

### Current Behavior
1. User is assigned to project (via bulk invite, direct assignment, or invitation acceptance)
2. User sees project on dashboard but NO pending forms
3. User must navigate to form editor URL
4. System creates ProjectFormSubmission via `GetOrCreateFormSubmissionCommand` (lazy creation)
5. Form now appears on dashboard

### Desired Behavior
1. User is assigned to project
2. System automatically creates ProjectFormSubmission for active form collection stages
3. User immediately sees pending forms on dashboard
4. Consistent experience for all users (not just seeded demo users)

## Context & Background

### Current System Architecture
- **Form Creation**: Lazy (on-demand via `GetOrCreateFormSubmissionCommand`)
- **Demo Seed Data**: Manual records via `012.SeedProjectFormSubmissions.sql`
- **Entry Points**: 3 paths for user assignment
  1. `AssignUserToProjectOrchestrationCommand` - assign existing user
  2. `BulkInviteParticipantsCommand` - bulk CSV import
  3. `ProjectInvitationAcceptedHandler` - invitation acceptance
- **Integration Event**: All paths publish `UserAddedToProjectIntegrationEvent`

### Domain Model
- **ProjectStageType Enum**:
  - `InitialFormCollection = 2` → Creates Start phase forms
  - `FinalFormCollection = 4` → Creates Final phase forms
- **ProjectFormSubmission**: Entity in BusinessIncubator domain
  - Phase: QuestionPhase enum (Start=1, Final=2, Undefined=4)
  - Status: Draft → Submitted → Approved/Rejected
  - Links to ProjectStage, Project, ParticipantUser

## Requirements

### Functional Requirements

**FR-1**: When a user is assigned to a project, the system MUST automatically create ProjectFormSubmission records for active form collection stages.

**FR-2**: Form submission creation MUST be idempotent (no duplicate submissions on re-assignment).

**FR-3**: System MUST only create submissions for relevant stages:
- Stage Type = 2 (InitialFormCollection) → Phase = Start
- Stage Type = 4 (FinalFormCollection) → Phase = Final

**FR-4**: System MUST check stage status before creating submissions:
- Stage IsActive = true
- Current date within stage StartDate/EndDate window

**FR-5**: Form submissions MUST be created with default values matching seed data:
- Status = Draft (1)
- TotalQuestions = 0
- AnsweredQuestions = 0
- CompletionPercentage = 0
- FormSchemaVersion = from ProjectKnowledgeStructure.CurrentVersion
- ProjectStageId = active stage ID

### Non-Functional Requirements

**NFR-1**: Implementation MUST use existing integration event pattern (`UserAddedToProjectIntegrationEvent`).

**NFR-2**: Implementation MUST NOT modify existing orchestration commands or controllers.

**NFR-3**: Form creation failures MUST NOT block user assignment operations (graceful degradation).

**NFR-4**: Implementation MUST log all form creation attempts and failures for troubleshooting.

**NFR-5**: Existing `GetOrCreateFormSubmissionCommand` MUST remain as safety net fallback.

### Scope Constraints

**In Scope**:
- Create integration event handler in BusinessIncubator domain
- Automatic form creation for InitialFormCollection and FinalFormCollection stages
- Support for all user assignment flows (bulk, direct, invitation)
- Idempotency checks

**Out of Scope**:
- Notification/email when forms are created
- Form pre-population with data
- Changes to existing form submission logic
- Changes to dashboard display logic
- Support for other stage types (Mentoring, Closure, etc.)

## Acceptance Criteria

**AC-1**: Given a new user is assigned to a project with an active InitialFormCollection stage
When the assignment completes
Then a ProjectFormSubmission record is created with Phase=Start and Status=Draft

**AC-2**: Given an existing user is assigned to a project with active FinalFormCollection stage
When the assignment completes
Then a ProjectFormSubmission record is created with Phase=Final and Status=Draft

**AC-3**: Given a user is assigned to the same project twice
When the second assignment completes
Then NO duplicate ProjectFormSubmission is created (idempotency)

**AC-4**: Given a project has NO active form collection stages
When a user is assigned to the project
Then NO ProjectFormSubmission records are created

**AC-5**: Given a project has multiple active form stages (e.g., Start and Final both active)
When a user is assigned to the project
Then ProjectFormSubmission records are created for ALL active form collection stages

**AC-6**: Given form creation fails for any reason
When the error occurs
Then user assignment still succeeds (graceful degradation)

**AC-7**: Given the integration event handler is invoked
When processing completes (success or failure)
Then appropriate log entries are created for troubleshooting

## Technical Design

### Solution Architecture

**Pattern**: Integration Event Handler
**Domain**: BusinessIncubator.Application
**Event**: `UserAddedToProjectIntegrationEvent` (already published by all assignment flows)

### Component Design

**New File**: `BusinessIncubator.Application/IntegrationEventHandlers/UserAddedToProjectHandler.cs`

**Dependencies**:
- `IBusinessIncubatorRepository` - fetch project, stages, check existing submissions
- `ITimeProvider` - current date/time for stage window validation
- `ILogger` - audit trail and troubleshooting

**Processing Logic**:
```
1. Receive UserAddedToProjectIntegrationEvent
2. Validate user role requires forms (Starter, Coordinator roles)
3. Fetch project with stages and knowledge structure
4. Find active form collection stages (Type=2 or Type=4)
5. For each active stage within time window:
   a. Determine phase from stage type
   b. Check if submission already exists (idempotency)
   c. If not exists, create ProjectFormSubmission
   d. Save to repository
6. Log results (success/warning/error)
```

### Error Handling Strategy
- **Project not found**: Log warning, continue (don't fail assignment)
- **No knowledge structure**: Log warning, continue (forms can't be created anyway)
- **No active stages**: Normal case, log info, no action needed
- **Database save failure**: Log error with full context, continue (GetOrCreateFormSubmissionCommand serves as fallback)

### Data Validation
- Stage.IsActive = true
- Stage.StartDate <= currentDate <= Stage.EndDate
- Stage.Type in [2, 4] (InitialFormCollection or FinalFormCollection)
- FormSchemaVersion > 0 (from knowledge structure)
- No existing submission for (ProjectId, UserId, Phase) combination

## Implementation Plan

### Phase 1: Create Event Handler (30 minutes)
1. Create `UserAddedToProjectHandler.cs` in `BusinessIncubator.Application/IntegrationEventHandlers/`
2. Implement `INotificationHandler<UserAddedToProjectIntegrationEvent>`
3. Add constructor with dependencies (repository, timeProvider, logger)
4. Implement `Handle` method with core logic
5. Add comprehensive logging

### Phase 2: Registration & Testing (15 minutes)
1. Verify MediatR automatically discovers handler (assembly scanning)
2. Manual verification: Check `DependencyInjection.cs` MediatR registration
3. Build and verify no compilation errors

### Testing Checklist
- [ ] Create new user and assign to project → form appears immediately
- [ ] Bulk invite 5 users → all 5 get forms automatically
- [ ] Accept project invitation → form appears immediately
- [ ] Assign same user to project twice → no duplicate forms
- [ ] Project with no active stage → no forms created (no error)
- [ ] Project with active InitialFormCollection → Start phase form created
- [ ] Project with active FinalFormCollection → Final phase form created
- [ ] Project with both stages active → both forms created

## Success Metrics

**Immediate**:
- Zero build errors/warnings
- All acceptance criteria passing
- Clean logs with no unexpected errors

**Post-Deployment**:
- User feedback: "I can see forms immediately after being added"
- Support tickets: Reduced "where is my form?" inquiries
- System consistency: All users have same experience (not just demo users)

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Event not published for some flows | High | Low | All 3 flows already publish event (verified) |
| Duplicate forms created | Medium | Low | Idempotency check before creation |
| Performance impact (many users) | Low | Low | Async event processing, simple queries |
| Integration event handler not discovered | High | Low | MediatR assembly scanning automatic |

## Dependencies

**External**:
- None (uses existing infrastructure)

**Internal**:
- `UserAddedToProjectIntegrationEvent` - ✅ Already published by all flows
- `IBusinessIncubatorRepository` - ✅ Has all needed methods
- `ProjectFormSubmission.CreateForPhase` - ✅ Factory method exists
- MediatR integration event infrastructure - ✅ Already configured

## Rollback Plan

If issues arise post-deployment:
1. Feature can be disabled by removing handler registration
2. Existing `GetOrCreateFormSubmissionCommand` continues to work as fallback
3. No database schema changes required (uses existing tables)
4. No breaking changes to existing code

## Documentation Updates

**Code Documentation**:
- XML comments on handler class and methods
- Inline comments for complex logic (stage type → phase mapping)

**Knowledge Base**:
- Update `.claude/common-issues.md` with troubleshooting guide
- Update `.claude/architecture.md` integration event section

## References

**Related Requirements**:
- REQ-003: Notification system (future - notify users when forms available)

**Code References**:
- `BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectFormSubmission:172-180` - GetPhaseForStage method
- `Shared.Application.IntegrationEvents.Auth.UserAddedToProjectIntegrationEvent` - Event definition
- `BusinessIncubator.Application.ProjectFormSubmissions.Commands.GetOrCreateFormSubmission.GetOrCreateFormSubmissionCommand:44-179` - Existing lazy creation logic
- `PostDeployment/012.SeedProjectFormSubmissions.sql:92-132` - Seed data pattern to replicate

**Architecture Decisions**:
- ADR-001: Integration Events in Modular Monolith (pattern justification)

---

**Status Notes**:
- Created: 2025-10-22
- Implementation: Ready to start
- Expected completion: Same day (~45 minutes)
