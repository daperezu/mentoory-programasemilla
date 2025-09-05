# ADR-002: Project Stage Management and Form Access Control

**Status**: Accepted  
**Date**: 2025-08-22  
**Decision Makers**: Development Team

## Context

The LinaSys platform needs to manage participant form submissions across different phases of a project lifecycle. Currently, there's no mechanism to control when participants can access and submit forms based on project timeline. We need to implement stage-based access control while respecting DDD boundaries and maintaining clean architecture.

### Current State
- Projects have simple Active/Archived status
- No time-based access control for forms
- No clear project lifecycle management
- Forms can be accessed anytime by participants

### Requirements
- Projects need defined stages (Invitation, Initial Forms, Mentoring, Final Forms, Closure)
- Each stage has specific date ranges
- Form access restricted to appropriate stages
- Task assignment based on current stage
- Integration with existing domains without violations

## Decision

### 1. Project Stages as Part of BusinessIncubator Domain

We will implement `ProjectStage` as an entity within the `Project` aggregate in the BusinessIncubator domain.

**Rationale**:
- Stages are intrinsically linked to project lifecycle
- Project aggregate naturally owns its timeline
- Stages affect project behavior (form access, task creation)
- Maintains aggregate consistency

**Implementation**:
```csharp
public class Project : SoftDeletableEntity
{
    private readonly List<ProjectStage> _projectStages = [];
    
    public ProjectStage AddStage(...)
    public ProjectStage? GetCurrentStage(DateTime currentDate)
    public bool IsInStage(ProjectStageType type, DateTime currentDate)
}
```

### 2. Stage-Based Access Control via Application Service

Access control will be implemented through `IProjectStageAuthorizationService` in the Application layer.

**Rationale**:
- Separates authorization logic from domain
- Allows complex cross-aggregate queries
- Testable in isolation
- Reusable across controllers

**Implementation**:
```csharp
public interface IProjectStageAuthorizationService
{
    Task<ProjectStageAccessResult> CanAccessFormAsync(
        long projectId, 
        string userId, 
        QuestionPhase phase);
}
```

### 3. Task Creation via Integration Events

When a participant accepts an invitation, task creation happens through integration events.

**Rationale**:
- Maintains domain boundary between invitation and tasks
- Allows eventual consistency
- Decouples invitation acceptance from task creation
- Enables future event sourcing

**Flow**:
1. `ProjectInvitation.Accept()` raises `ProjectInvitationAccepted` event
2. `ProjectInvitationAcceptedHandler` creates appropriate `StarterTask`
3. Task type determined by current project stage

### 4. Form Phase Determination by Stage

The form phase (Initial/Final) is determined by the current project stage, not stored separately.

**Rationale**:
- Single source of truth for project timeline
- Prevents phase/stage mismatch
- Simplifies form access logic
- Stage drives behavior

**Mapping**:
- `InitialFormCollection` stage → `QuestionPhase.Start`
- `FinalFormCollection` stage → `QuestionPhase.Final`

### 5. Enhanced ProjectFormSubmission with Progress

`ProjectFormSubmission` enhanced with progress tracking and stage reference.

**Rationale**:
- Tracks completion percentage for better UX
- Links submission to specific stage
- Enables auto-save timestamps
- Supports draft management

**Enhancements**:
```csharp
public class ProjectFormSubmission
{
    public QuestionPhase Phase { get; private set; }
    public long? ProjectStageId { get; private set; }
    public int CompletionPercentage { get; private set; }
    public DateTime? LastAutoSaveAt { get; private set; }
}
```

## Consequences

### Positive
- Clear project lifecycle management
- Enforced time-based access control
- Better user experience with progress tracking
- Maintains DDD boundaries
- Extensible for future stage types
- Audit trail for stage changes

### Negative
- Additional complexity in Project aggregate
- Database migration required
- More integration events to manage
- Stage configuration UI needed for administrators

### Neutral
- Stage dates must be managed carefully
- Coordinators need training on stage management
- Existing projects need stage migration

## Alternatives Considered

### Alternative 1: Separate Stage Aggregate
**Rejected because**: Would require complex cross-aggregate transactions and could lead to consistency issues between Project and Stage states.

### Alternative 2: Stage as Value Object
**Rejected because**: Stages have identity, lifecycle, and behavior that requires entity semantics.

### Alternative 3: Hard-coded Stage Dates
**Rejected because**: Lacks flexibility for different project timelines and would require code changes for adjustments.

### Alternative 4: Stage Management in Separate Domain
**Rejected because**: Would violate domain cohesion as stages are core to project lifecycle.

## Implementation Notes

### Database Design
- New table: `bi.ProjectStages`
- Foreign key to Projects
- Unique constraint on (ProjectId, Type)
- Check constraint for date validity

### Migration Strategy
1. Deploy stage tables
2. Create default stages for existing projects
3. Enable stage-based access control
4. Monitor and adjust

### Testing Requirements
- Unit tests for stage overlaps
- Integration tests for access control
- E2E tests for complete flow
- Performance tests for concurrent access

## References
- [DDD Aggregate Design](https://martinfowler.com/bliki/DDD_Aggregate.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- ADR-001: Integration Events for Cross-Domain Communication
- `.claude/ddd-patterns.md`