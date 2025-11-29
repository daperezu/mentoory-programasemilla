# LinaSys Project Memory

## Project Overview
- **Platform**: ASP.NET Core 9 business incubator management system
- **Database**: SQL Server with Entity Framework
- **Architecture**: Clean Architecture with Domain-Driven Design
- **Cloud Native**: .NET Aspire for orchestration and observability
- **Frontend**: Razor Views with Bootstrap 5
- **Authentication**: Microsoft Identity
- **Language**: Spanish UI (all user-facing text)

## Key Commands
- **Build**: `dotnet build`
- **Run with Aspire**: `dotnet run --project Aspire.AppHost`
- **Run Web Only**: `dotnet run --project LinaSys.Web`
- **Test**: `dotnet test`
- **Database Build**: `cd Db && dotnet build` (generates DACPAC with PostDeployment scripts)
- **Infrastructure**: `docker compose --file infrastructure-docker-compose.yml up -d`

## 📚 Knowledge Base
**Quick lookup by scenario:**

| Scenario | Documentation |
|----------|---------------|
| Creating features | [architecture.md](.claude/architecture.md), [web-patterns.md](.claude/web-patterns.md) |
| Domain changes | [ddd-patterns.md](.claude/ddd-patterns.md), [domain-reference.md](.claude/domain-reference.md) |
| Build errors | [coding-standards.md](.claude/coding-standards.md), [common-issues.md](.claude/common-issues.md) |
| Cross-domain work | [ADR-001-integration-events.md](.claude/architecture-decisions/ADR-001-integration-events.md) 

## 📋 Requirements
- **Active**: `.claude/requirements/active/` ← Current work items
- **Pending**: `.claude/requirements/pending/` ← Next up
- **Completed**: `.claude/requirements/completed/` ← Done archive

### Currently Active
- 📝 Check `.claude/requirements/active/` for current work items

### Pending Implementation
- 📝 Check `.claude/requirements/pending/` for additional work

### Recently Completed
- ✅ **REQ-016**: Automatic Project Form Submission Creation (2025-10-22)
- ✅ **REQ-015**: Manage Project Assignments for Existing Users (2025-10-16)
- ✅ **REQ-013**: Registration Email Refactoring (2025-01-18)
- ✅ **REQ-012**: Phoenix Homepage Redesign with Dual Discovery Modes (2025-01-16)
- ✅ **REQ-011**: Public Homepage with Geolocation-Based Project Discovery (2025-01-15)
- ✅ **REQ-006**: Bidirectional Feedback Conversation System (2025-09-09)
- ✅ **REQ-005**: Modern Phoenix-Aligned Form Experience (2025-09-08)
- ✅ **REQ-004**: Modern Toast Notification System (2025-09-08)
- ✅ Inactivity logout component for ContextSelection page
- ✅ IApplicationUrlService GetLogoutUrl method implementation

## 🎯 Current Context
- **Branch**: `develop`
- **Status**: ✅ Clean state - Ready for next requirement
- **Build Status**: ✅ Clean build - 0 errors, 0 warnings (All projects)
- **Session File**: `.claude/CURRENT_SESSION.md` ← *Start here for today's work*
- **Full History**: `.claude/WORK_LOG.md` ← *Detailed progress archive*

### Active Requirements
- **None** - Ready for new work

### Requirements in Analysis
- **REQ-014**: Aspire Runtime Optimization (Memory: 255MB → 50MB target)
  - Status: In analysis phase (see `.claude/requirements/analysis/`)
  - Not yet approved for active implementation

### Recently Completed
- ✅ **REQ-016**: Automatic Project Form Submission Creation (2025-10-22)
  - Created UserAddedToProjectHandler integration event handler
  - Forms now appear immediately when users assigned to projects
  - Build: 0 errors, 0 warnings
- ✅ **REQ-015**: Manage Project Assignments for Existing Users (2025-10-16)
- ✅ **REQ-013**: Registration Email Refactoring (2025-01-18)
- ✅ **REQ-012**: Phoenix Homepage Redesign with Dual Discovery Modes (2025-01-16)

### What's Next
1. **Review Pending Requirements**: Check `.claude/requirements/pending/` for next priorities
2. **User Decision Required**: Choose next requirement to work on
3. **Or**: Provide new requirement for implementation

## Critical Reminders
- ⚠️ **Zero Warnings Policy**: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- 🔒 **Security**: After adding controllers/actions, update `001.SeedWebFeatures.sql`
- 🌍 **Spanish Only**: All UI text, messages, and validation in Spanish
- 🏗️ **Clean Architecture**: No web dependencies in Domain/Application layers
- ✅ **Clean Build Required**: Fix all errors and warnings before committing
- 📁 **Database PostDeployment**: Located at `/PostDeployment/` (project root, outside `Db/` to avoid MSBuild.Sdk.SqlProj validation)

## Quick Reference

### File Organization
```
/Areas/{AreaName}/Controllers|Models|Views  # Area-based structure
/Domain/Aggregates/{Aggregate}/            # Domain entities
/Application/{Feature}/Commands|Queries/   # CQRS operations
/Infrastructure/Persistence/               # EF Core implementations
```

### Common Patterns
```csharp
// Error handling in controllers
this.MapErrorsToModelStateAndSetErrorToast<T>(result);

// Success feedback
this.SetSuccessToast("Operación exitosa");

// File upload conversion
var command = new Command(file.OpenReadStream(), file.FileName);
```

### JavaScript & Static Files
- **Location**: All JavaScript files must be in `/wwwroot/js/` (NOT in Views folders)
- **Naming**: Use descriptive names like `projectknowledgestructure-edit.js`
- **Path**: Reference as `~/js/businessincubators/filename.js` in views

### Tree View Implementation
- **Library**: jsTree 3.3.12 for hierarchical data visualization
- **Node ID Handling**: Always use helper functions for ID extraction (may be in `node.data.id` or `node.data.entityId`)
- **Entity Loading**: Include all related entities in EF queries (e.g., `.ThenInclude(q => q.ProjectAnswerOptions)`)
- **Node Types**: Define all types in tree configuration (root, structure, block, module, topic, subject, question, answer)

### Need Help?
1. Check `.claude/` documentation first
2. Look for similar patterns in existing code
3. Verify requirements in `.claude/requirements/`
4. Run `dotnet build` to catch issues early

## Project Context Notes
- **System Status**: Not in production yet - direct schema changes allowed, no migration scripts needed
- **Base Branch**: Always work from `develop`, not `main`
- **Documentation Updates**: When learning new patterns, update the appropriate `.claude/` documentation file

## Known Issues & Solutions

### Form Submission Flow

#### Prerequisites for Forms to Appear
1. **ProjectKnowledgeStructure**: Project must have configured questions and answer options
2. **Active ProjectStage**: Stage must be active with current date within its window
3. **User Access**: User must be a project participant with incubator access

#### Current System Design (Automatic Creation - REQ-016)
✅ **Forms are automatically created** when users are assigned to projects:
1. Coordinator assigns user to project (or user accepts invitation)
2. System detects active form collection stages
3. `UserAddedToProjectHandler` automatically creates ProjectFormSubmission records
4. Forms appear immediately on user's dashboard
5. Fallback: `GetOrCreateFormSubmissionCommand` still works if handler fails

**Benefits**:
- ✅ No delay between user assignment and form availability
- ✅ Consistent user experience
- ✅ Idempotent (no duplicate forms if user reassigned)
- ✅ Graceful error handling (doesn't block user assignment)

#### Still Missing Components
- No background monitoring of stage activations (forms created only during user assignment)
- No automatic notification when forms become available
- No integration events for stage lifecycle changes
- See **REQ-003** for planned notification system implementation