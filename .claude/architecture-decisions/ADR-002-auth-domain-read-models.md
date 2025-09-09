# ADR-002: Auth Domain Read Models for Access Control

## Status
Accepted

## Context
The Auth domain needs to determine user access to projects and incubators. Initially, Auth services were using raw SQL queries to directly query the `businessincubators` schema, violating DDD boundaries and creating tight coupling between domains.

### Initial Incorrect Approaches Considered
1. **Moving tables to Auth domain**: We considered moving `ProjectUsers` and `ProjectMentorAssignments` tables to the Auth domain, but this would break the Project aggregate boundary since these entities contain business logic and are part of the aggregate's consistency boundary.

2. **Direct cross-domain queries**: Using raw SQL to query other domains' tables violates Clean Architecture principles and creates maintenance issues.

## Decision
We will implement **read models in the Auth domain** that are synchronized through **integration events** from the BusinessIncubator domain.

### Architecture Pattern
```
BusinessIncubator Domain (Owns Business Logic):
├── Project Aggregate
│   ├── ProjectUsers (business relationship)
│   ├── AddUser() (domain method with business rules)
│   └── RemoveUser() (domain method with business rules)
├── ProjectMentorAssignments (business entity)
└── Publishes: Integration Events when relationships change

Auth Domain (Owns Access Control):
├── UserProjectAccess (read model for access control)
├── UserIncubatorAccess (read model for access control)
├── UserMentorshipAccess (read model for access control)
└── Handles: Integration Events → Updates read models synchronously
```

### Key Principles
1. **Business logic stays with aggregates**: `ProjectUsers` remains part of the Project aggregate because it contains business rules (e.g., user roles, activation status)
2. **Read models for queries**: Auth domain maintains its own read models optimized for access control queries
3. **Event-driven synchronization**: Changes in business relationships trigger events that update read models
4. **Synchronous consistency**: Event handlers update read models synchronously to maintain consistency

## Implementation

### Read Model Structure
```csharp
// Auth.Domain/ReadModels/UserProjectAccess.cs
public class UserProjectAccess : Entity
{
    public string UserId { get; private set; }
    public long ProjectId { get; private set; }
    public long IncubatorId { get; private set; }
    public string Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    
    public static UserProjectAccess Create(
        string userId, 
        long projectId, 
        long incubatorId, 
        string role)
    {
        return new UserProjectAccess
        {
            UserId = userId,
            ProjectId = projectId,
            IncubatorId = incubatorId,
            Role = role,
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow
        };
    }
}
```

### Integration Events
```csharp
// BusinessIncubator.Application/IntegrationEvents/UserAddedToProjectIntegrationEvent.cs
public record UserAddedToProjectIntegrationEvent(
    string UserId,
    long ProjectId,
    long IncubatorId,
    string Role,
    DateTime OccurredAt) : IIntegrationEvent;
```

### Event Flow
1. Business operation: `Project.AddUser()` is called
2. Command handler publishes `UserAddedToProjectIntegrationEvent`
3. Auth domain handler receives event synchronously
4. Handler updates `UserProjectAccess` read model
5. Auth services query local read models with LINQ

## Consequences

### Positive
- **Clean domain boundaries**: Each domain owns its core concepts
- **No cross-domain queries**: Eliminates raw SQL queries between schemas
- **Type safety**: LINQ queries instead of string SQL
- **Maintainability**: Clear separation of business logic and access control
- **Performance**: Read models can be optimized for queries
- **Eventual microservices**: Easy to extract domains to separate services

### Negative
- **Data duplication**: Same information exists in multiple places
- **Synchronization complexity**: Must ensure events are published for all changes
- **Eventual consistency risk**: If events fail, read models become stale
- **More code**: Additional event handlers and read models

### Mitigation Strategies
1. **Synchronous handlers**: Use synchronous event handling for critical access control updates
2. **Audit logging**: Log all events for debugging synchronization issues
3. **Validation**: Add integration tests to verify synchronization
4. **Monitoring**: Track event processing metrics

## Alternatives Rejected

### Alternative 1: Move Tables to Auth Domain
**Why rejected**: `ProjectUsers` is part of the Project aggregate with business rules. Moving it would break the aggregate boundary and scatter business logic.

### Alternative 2: Direct Service Calls
**Why rejected**: Would create tight coupling between domains and make it harder to extract to microservices later.

### Alternative 3: Shared Database Views
**Why rejected**: Still creates schema-level coupling and doesn't work well with EF Core.

## Migration Strategy

Since the system is not in production:
1. Create new read model tables in Auth schema
2. Update seed scripts to populate read models
3. No data migration needed - just recreate database

For future production systems:
1. Deploy read models alongside existing code
2. Dual-write period: Update both old and new tables
3. Migrate historical data
4. Switch reads to new models
5. Remove old code

## References
- ADR-001: Integration Events in Modular Monolith
- DDD Patterns documentation: `.claude/ddd-patterns.md`
- Clean Architecture guide: `.claude/architecture.md`

## Decision Makers
- Development Team
- Architecture Review

## Date
2025-01-10