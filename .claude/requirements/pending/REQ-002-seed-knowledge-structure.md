# REQ-002: Seed Data for Project Knowledge Structure

## Status
**Pending** - Awaiting implementation

## Priority
**High** - Blocks starter users from accessing forms

## Description
The demo project requires seed data for ProjectKnowledgeStructure components to enable form functionality for starter users. Currently, the demo.starter user cannot see or fill forms because the required knowledge structure entities are missing.

## Problem Statement
When a starter user (demo.starter) logs in and navigates to the Participant Dashboard (`/Participant/Dashboard`), no forms are displayed in the "Pending Forms" section. The root cause is:

1. `GetOrCreateFormSubmissionCommand` fails at lines 70-78 when checking for ProjectKnowledgeStructure
2. `GetPendingFormsQuery` returns empty results because no forms can be created without the knowledge structure
3. The demo project in seed data has no associated ProjectKnowledgeStructure, ProjectBlocks, or ProjectQuestions

## Requirements

### Functional Requirements
1. Create seed data for the demo project's knowledge structure including:
   - One ProjectKnowledgeStructure entity linked to the demo project
   - At least 2 ProjectBlocks (e.g., "Información General", "Plan de Negocio")
   - Minimum 5 ProjectQuestions per block with varied answer types
   - ProjectAnswerOptions for multiple-choice questions
   - Appropriate QuestionPhase values (Start phase for initial forms)

2. Ensure seed data includes:
   - Questions with different AnswerType values (Text, Number, SingleChoice, MultipleChoice)
   - IsUsedForDiagnosis flag set for key questions
   - Proper Order values for sequential display
   - Spanish text for all user-facing content

### Technical Requirements
1. Add seed data to `Db\PostDeployment\` folder
2. Follow existing seed file patterns (idempotent, uses MERGE statements)
3. Include verification queries to confirm data insertion
4. Maintain referential integrity with existing demo project

### Data Structure Example
```sql
-- ProjectKnowledgeStructure
INSERT INTO [businessincubators].[ProjectKnowledgeStructures]
    (ProjectId, Name, Description, CurrentVersion, IsLocked)

-- ProjectBlocks  
INSERT INTO [businessincubators].[ProjectBlocks]
    (ProjectId, Name, Order)

-- ProjectQuestions
INSERT INTO [businessincubators].[ProjectQuestions]
    (ProjectBlockId, Text, AnswerType, Order, IsUsedForDiagnosis, AppliesToPhase)

-- ProjectAnswerOptions
INSERT INTO [businessincubators].[ProjectAnswerOptions]
    (ProjectQuestionId, Text, Order)
```

## Acceptance Criteria
1. [ ] Seed file creates ProjectKnowledgeStructure for demo project
2. [ ] At least 10 questions total across 2+ blocks
3. [ ] Questions have appropriate phase assignments (Start/Final/Both)
4. [ ] All text content is in Spanish
5. [ ] demo.starter user sees forms in Dashboard after seed execution
6. [ ] Seed script is idempotent (can run multiple times safely)
7. [ ] Build succeeds with 0 errors, 0 warnings

## Implementation Notes
- The user has manually configured ProjectStages (not part of this requirement)
- Focus only on knowledge structure components
- Reference existing project ID from 006.SeedStarterData.sql
- Consider creating a separate file like 010.SeedProjectKnowledgeStructure.sql

## Dependencies
- Requires demo project to exist (006.SeedStarterData.sql)
- ProjectStages must be configured (user handles manually)

## Testing
1. Deploy fresh database
2. Run all seed scripts including new knowledge structure seed
3. Login as demo.starter (password: Nvxcrsm19!)
4. Navigate to /Participant/Dashboard
5. Verify forms appear in "Formularios Pendientes" section

## References
- GetOrCreateFormSubmissionCommand.cs:70-78 (knowledge structure check)
- GetPendingFormsQuery.cs (form retrieval logic)
- ProjectFormSubmission.cs (form creation requirements)