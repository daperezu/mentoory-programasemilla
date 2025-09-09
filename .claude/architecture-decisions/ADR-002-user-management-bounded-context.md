# ADR-002: User Management as Separate Bounded Context

## Status
Proposed

## Context
The current proposal places UserProfiles in the Auth domain, mixing authentication concerns (Identity, login, roles) with business user management (profiles, assignments, preferences). This violates DDD principles of bounded contexts and single responsibility.

### Problems with Current Approach
1. **Auth domain pollution**: Authentication domain contains business logic
2. **Cross-domain coupling**: Direct references to projects/incubators from Auth
3. **Identity conflation**: Business operations (profile updates) mixed with Identity operations
4. **Scalability issues**: Cannot extract Auth to microservice without taking business logic

## Decision
Create a new **UserManagement** bounded context separate from Auth domain, with all cross-domain operations coordinated through the Orchestration layer.

### New Architecture
```
Auth Domain (Authentication Only)
├── Microsoft Identity (unchanged)
├── Roles and permissions
├── Login/logout/tokens
└── Read models for access control

UserManagement Domain (NEW)
├── UserProfile aggregate
├── User preferences
├── Avatar management
├── Location value objects
└── Profile administration

Orchestration Layer
├── User creation flow
├── Assignment coordination
├── Email/PIN change flows
└── Cross-domain synchronization
```

## Consequences

### Positive
- **Clean boundaries**: Each domain has single responsibility
- **Microservice ready**: Can extract any domain independently
- **Testability**: Isolated domain logic easier to test
- **Maintainability**: Clear ownership and reduced coupling
- **Scalability**: Can scale domains independently
- **DDD compliance**: Proper aggregate boundaries maintained

### Negative
- **Additional complexity**: New bounded context to maintain
- **More code**: Orchestration commands for cross-domain operations
- **Learning curve**: Team needs to understand new structure
- **Migration effort**: Existing data needs migration

### Mitigation
1. **Phased migration**: Move incrementally, maintain backward compatibility
2. **Clear documentation**: Document boundaries and responsibilities
3. **Team training**: DDD workshops for development team
4. **Automated tests**: Comprehensive test coverage for migration

## Implementation Details

### Domain Events Flow
```csharp
// User creation flow through Orchestration
1. CreateUserOrchestrationCommand
   ├── CreateIdentityUserCommand (Auth)
   ├── CreateUserProfileCommand (UserManagement)
   ├── AssignRolesToUserCommand (Auth)
   ├── UserCreatedIntegrationEvent (Published)
   └── SendWelcomeEmailCommand (Notification)

// Assignment flow
2. AssignUserToProjectCommand (Orchestration)
   ├── ValidateProjectExistsQuery (BusinessIncubator)
   ├── ValidateUserExistsQuery (UserManagement)
   ├── UserAssignedToProjectIntegrationEvent (Published)
   └── Update read models (Auth domain)
```

### Database Changes
```sql
-- New schema
CREATE SCHEMA [usermanagement];

-- Move UserProfiles from auth to usermanagement
-- No foreign keys between schemas (domain boundaries)
```

## Alternatives Considered

### 1. Keep UserProfiles in Auth Domain
- ❌ Violates bounded context principles
- ❌ Mixes authentication with business logic
- ❌ Makes microservice extraction impossible

### 2. Put UserProfiles in BusinessIncubator Domain
- ❌ Users exist beyond single incubator
- ❌ Would create circular dependencies
- ❌ Not all users belong to incubators

### 3. Generic "Users" Domain
- ✅ Clean separation
- ✅ Single responsibility
- ❌ "Users" is too generic, prefer "UserManagement" for clarity

## Migration Path

### Phase 1: Create New Structure
1. Create UserManagement domain projects
2. Implement UserProfile aggregate
3. Create orchestration commands
4. Add integration event handlers

### Phase 2: Dual Write
1. Write to both old and new locations
2. Verify data consistency
3. Update read operations to new source

### Phase 3: Cutover
1. Switch all operations to new domain
2. Remove old code
3. Clean up database

## Validation Criteria
- All existing functionality preserved
- No performance degradation
- Zero data loss during migration
- All tests passing
- Clean build with zero warnings

## References
- [DDD Patterns](.claude/ddd-patterns.md)
- [Architecture Guide](.claude/architecture.md)
- [User Administration Requirements](.claude/requirements/active/user-administration-implementation.md)
- Evans, Eric. "Domain-Driven Design" (2003)

## Decision Date
2024-01-24

## Participants
- Development Team
- Architecture Review

## Notes
This decision prioritizes proper DDD boundaries over implementation simplicity. The additional complexity is justified by improved maintainability, testability, and future scalability.