# Work Log

## 2025-09-06 - Form Submission Troubleshooting & Seed Data Fixes

### Context
User reported that demo.starter couldn't see forms on Participant Dashboard after login. Investigated the entire form creation flow.

### Completed
1. **Fixed EF Core Include error** in `BusinessIncubatorRepository.cs`:
   - Issue: `Include(p => p.ProjectStages)` failed with "invalid expression" error
   - Root cause: ProjectStages property was explicitly ignored in DbContext (line 651-652)
   - Solution: Changed to `.Include("_projectStages")` to use private backing field
   - File: `BusinessIncubator.Infrastructure\Persistence\Repositories\BusinessIncubatorRepository.cs:907`

2. **Fixed UserIncubatorAccess seed data**:
   - Issue: Only Coordinators were getting incubator access in seed
   - Impact: demo.starter (Starter role) had no incubator access
   - Solution: Modified MERGE query to include ALL demo users (Starter, Mentor, Coordinator)
   - File: `Db\PostDeployment\007.SeedAuthAccessTables.sql:75-99`

3. **Root cause analysis** for missing forms:
   - Traced through GetPendingFormsQuery ’ GetOrCreateFormSubmissionCommand
   - Found failure at line 70-78: No ProjectKnowledgeStructure exists
   - Verified: No seed data for ProjectKnowledgeStructure, ProjectBlocks, or ProjectQuestions

### Key Decisions
1. **Document as requirement, not immediate fix**: Created REQ-002 instead of rushing seed implementation
2. **User manages ProjectStages**: Confirmed stages are user-configured post-deployment, not seeded
3. **Private field Include pattern**: Use string names for EF Core to access private backing fields

### Problems & Solutions
**Problem**: EF Core couldn't use Include with public read-only collection properties
```csharp
// Failed:
.Include(p => p.ProjectStages)  // ProjectStages is public IReadOnlyCollection

// Solution:
.Include("_projectStages")  // Use private backing field name
```

**Problem**: Starter users had no incubator access
```sql
-- Before: Only Coordinators
WHERE pu.Role = 'Coordinator' AND pu.IsActive = 1

-- After: All active users
WHERE pu.IsActive = 1
AND pu.UserId IN (@DemoStarterId, @DemoMentorId, @DemoCoordinatorId)
```

### Patterns Discovered
1. **EF Core with DDD encapsulation**: When using private collections with public read-only accessors, use string-based Include
2. **Seed data dependencies**: Auth domain read models must mirror BusinessIncubator relationships
3. **Form creation requirements**: ProjectKnowledgeStructure is mandatory, not optional

### Documentation Updates
- Created `REQ-002-seed-knowledge-structure.md` in pending requirements
- Updated `CLAUDE.md` with "Known Issues & Solutions" section
- Added REQ-002 to pending implementation list

### Next Steps
1. Implement REQ-002 seed data for ProjectKnowledgeStructure
2. Test full form creation flow with seed data
3. Verify demo.starter can see and fill forms

---

## 2025-09-03 - Session Initialization

### Initial Setup
- Documented REQ-001 for enhanced user creation feature
- Set up working session tracking