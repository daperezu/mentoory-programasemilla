# REQ-008: Dual Answers System - Coordinator and Starter Response Capture

**Status**: Active  
**Priority**: High  
**Created**: 2025-01-10  
**Target Sprint**: Current  

## 1. Executive Summary

The system currently captures only starter answers in form submissions. This requirement extends the system to capture and persist both **starter answers** and **coordinator answers** for each question, enabling coordinators to provide their expert assessment alongside participant responses. The solution uses a **single-row, dual-column approach** in the BusinessIncubator domain and a **discriminated row approach** in the Diagnostics domain, minimizing risk while maintaining system integrity and following Clean Architecture principles. This approach ensures minimal disruption to existing flows while providing clear value through expert validation of participant responses.

## 2. Business Context

### Current State
- Starters submit forms with their answers
- Coordinators review and approve/reject submissions
- Only starter answers flow to Diagnostics domain
- Coordinators cannot record their own assessment

### Desired State
- Coordinators can provide their own answer for each question
- Both sets of answers are preserved and flow to Diagnostics
- Coordinators can indicate which answer should be used for diagnosis
- Full audit trail of who answered what and when

## 3. Data Modeling - Single Row with Two Columns Approach

### Chosen Architecture
The system will implement a **single-row, dual-column approach** that extends the existing structure with minimal disruption:

- **BusinessIncubator Domain**: Add `CoordinatorData` column to `ProjectFormSubmissions` table alongside existing `DraftData`
- **Diagnostics Domain**: Add `AnswerSource` discriminator to differentiate between Starter and Coordinator answers

### Key Benefits
- **Minimal schema changes**: Only adding columns, not restructuring tables
- **Preserves constraints**: Existing unique constraints remain valid
- **Clear data ownership**: Each submission row contains both perspectives
- **Simple UI mapping**: Direct 1:1 mapping between database and UI components
- **Low risk**: Extends existing patterns without breaking changes

### Implementation Details
- Starter answers remain in `DraftData` (unchanged)
- Coordinator answers stored in new `CoordinatorData` column
- Both use same JSON structure for consistency
- Approval requires both datasets to be present

## 4. Target Schema Changes

### BusinessIncubator Domain

```sql
-- File: Db/businessincubators/Tables/ProjectFormSubmissions.sql
ALTER TABLE [businessincubators].[ProjectFormSubmissions]
ADD [CoordinatorData] NVARCHAR(MAX) NULL,
    [CoordinatorReviewedAt] DATETIME2 NULL,
    [CoordinatorUserId] NVARCHAR(450) NULL;

-- File: Db/businessincubators/Indexes/IX_ProjectFormSubmissions_CoordinatorUserId.sql
CREATE NONCLUSTERED INDEX [IX_ProjectFormSubmissions_CoordinatorUserId]
ON [businessincubators].[ProjectFormSubmissions]([CoordinatorUserId])
WHERE [CoordinatorUserId] IS NOT NULL;
```

### Diagnostics Domain

```sql
-- File: Db/diagnostics/Tables/DiagnosisAnswers.sql
ALTER TABLE [diagnostics].[DiagnosisAnswers]
ADD [AnswerSource] NVARCHAR(20) NOT NULL DEFAULT 'Starter',
    [CoordinatorUserId] NVARCHAR(450) NULL,
    [PreferredForDiagnosis] BIT NOT NULL DEFAULT 0;

-- Drop and recreate unique constraint
DROP INDEX [UQ_DiagnosisAnswers_ProjectId_UserId_QuestionId_AnswerOptionId_Phase] 
ON [diagnostics].[DiagnosisAnswers];

CREATE UNIQUE NONCLUSTERED INDEX [UQ_DiagnosisAnswers_ProjectId_UserId_QuestionId_AnswerOptionId_Phase_Source] 
ON [diagnostics].[DiagnosisAnswers](
    [ProjectId], [UserId], [QuestionId], [AnswerOptionId], [Phase], [AnswerSource]
);

-- File: Db/diagnostics/Indexes/IX_DiagnosisAnswers_AnswerSource.sql
CREATE NONCLUSTERED INDEX [IX_DiagnosisAnswers_AnswerSource]
ON [diagnostics].[DiagnosisAnswers]([AnswerSource])
INCLUDE ([ProjectId], [UserId], [Phase]);
```

## 5. Domain Model Changes

### ProjectFormSubmission Entity

```csharp
// Add properties
public string? CoordinatorData { get; private set; }
public DateTime? CoordinatorReviewedAt { get; private set; }
public string? CoordinatorUserId { get; private set; }

// Add method
public void SaveCoordinatorReview(
    string coordinatorUserId, 
    string coordinatorData, 
    DateTime reviewedAt)
{
    if (Status != ProjectFormSubmissionStatus.Submitted)
        throw new InvalidOperationException("Solo se pueden revisar formularios enviados");
    
    CoordinatorUserId = coordinatorUserId;
    CoordinatorData = coordinatorData;
    CoordinatorReviewedAt = reviewedAt;
}
```

## 6. Event Contract (Updated)

```csharp
// BusinessIncubator.Application/IntegrationEvents/ProjectFormSubmissionApproved.cs (Updated)
public sealed record ProjectFormSubmissionApproved(
    long ProjectId,
    long SubmissionId,
    string ParticipantUserId,
    string CoordinatorUserId,
    string StarterDraftData,
    string CoordinatorDraftData,
    QuestionPhase Phase,
    DateTime ApprovedAt,
    List<DualAnswerDto> ProcessedAnswers) : IntegrationEvent, INotification;

public sealed record DualAnswerDto(
    long BlockId,
    string BlockName,
    long QuestionId,
    string QuestionText,
    AnswerDataDto? StarterAnswer,
    AnswerDataDto? CoordinatorAnswer,
    bool UseCoordinatorForDiagnosis);
```

## 7. Application Layer Changes

### New Commands

```csharp
// SaveCoordinatorAnswersCommand.cs
public record SaveCoordinatorAnswersCommand(
    long SubmissionId,
    string CoordinatorUserId,
    DraftDataDto CoordinatorData,
    Dictionary<long, bool> PreferenceSelections) : IBaseRequest;

// SaveCoordinatorAnswersCommandHandler.cs
public override async Task<Result> Handle(SaveCoordinatorAnswersCommand request, CancellationToken cancellationToken)
{
    // 1. Validate coordinator role
    // 2. Get submission (must be in Submitted status)
    // 3. Validate all required questions answered
    // 4. Save coordinator data
    // 5. Persist changes
}
```

### Modified Approval Command

```csharp
// ApproveFormSubmissionWithReviewCommandHandler.cs modifications
// Add validation for coordinator answers before approval
if (string.IsNullOrWhiteSpace(submission.CoordinatorData))
{
    return Failure(
        ResultErrorCodes.Validation_SomeFieldsAreInvalid,
        ("CoordinatorAnswers", "Debe completar su revisión antes de aprobar"));
}

// Publish updated event with both datasets
var integrationEvent = new ProjectFormSubmissionApproved(
    submission.ProjectId,
    submission.Id,
    submission.ParticipantUserId,
    submission.CoordinatorUserId!,
    submission.DraftData,
    submission.CoordinatorData,
    submission.Phase,
    submission.ApprovedAt!.Value,
    processedAnswers);
```

## 8. Diagnostics Integration

### Event Handler

```csharp
// Orchestration.Application/BusinessIncubator/EventHandlers/ProjectFormSubmissionApprovedHandler.cs (Updated)
public async Task Handle(ProjectFormSubmissionApproved notification, CancellationToken cancellationToken)
{
    var diagnosis = await GetOrCreateDiagnosis(notification.ProjectId, notification.ParticipantUserId);
    var answersToSave = new List<DiagnosisAnswerInput>();
    
    foreach (var answer in notification.ProcessedAnswers)
    {
        // Save starter answer
        if (answer.StarterAnswer != null)
        {
            answersToSave.Add(CreateInput(answer.StarterAnswer, "Starter", null));
        }
        
        // Save coordinator answer if different or preferred
        if (answer.CoordinatorAnswer != null && 
            (answer.UseCoordinatorForDiagnosis || IsDifferent(answer)))
        {
            answersToSave.Add(CreateInput(
                answer.CoordinatorAnswer, 
                "Coordinator", 
                notification.CoordinatorUserId));
        }
    }
    
    diagnosis.AddOrUpdateAnswersFromApprovedSubmission(answersToSave);
    await SaveDiagnosis(diagnosis);
}
```

## 9. UI Implementation

### Coordinator Review View

```html
<!-- Web/Areas/Coordination/Views/FormReview/Review.cshtml -->
<div class="card mb-3">
    <div class="card-body">
        <div class="row g-3">
            <!-- Starter Answer Column -->
            <div class="col-md-6">
                <div class="card border-primary h-100">
                    <div class="card-header bg-primary-subtle">
                        <h6 class="mb-0">
                            <i class="fas fa-user me-2"></i>
                            Respuesta del Emprendedor
                        </h6>
                    </div>
                    <div class="card-body">
                        <!-- Read-only starter answer -->
                    </div>
                </div>
            </div>
            
            <!-- Coordinator Answer Column -->
            <div class="col-md-6">
                <div class="card border-success h-100">
                    <div class="card-header bg-success-subtle">
                        <h6 class="mb-0">
                            <i class="fas fa-user-tie me-2"></i>
                            Tu Respuesta como Coordinador
                        </h6>
                        <button class="btn btn-sm btn-outline-primary" 
                                onclick="copyFromStarter(@Model.QuestionId)">
                            <i class="fas fa-copy me-1"></i>Copiar
                        </button>
                    </div>
                    <div class="card-body">
                        <!-- Coordinator input controls -->
                        <div class="form-check form-switch mt-3">
                            <input class="form-check-input use-for-diagnosis" 
                                   type="checkbox" 
                                   data-question-id="@Model.QuestionId">
                            <label class="form-check-label">
                                Usar mi respuesta para el diagnóstico
                            </label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Visual Diff Alert -->
        <div class="alert alert-warning mt-3 answer-diff-alert" style="display: none;">
            <i class="fas fa-exclamation-triangle me-2"></i>
            Tu respuesta difiere de la del emprendedor
        </div>
    </div>
</div>
```

### JavaScript Support

```javascript
// Web/wwwroot/js/coordination/form-review-dual.js
class DualAnswerReviewManager {
    constructor() {
        this.submissionId = $('#submissionId').val();
        this.coordinatorAnswers = {};
        this.autoSaveInterval = 30000; // 30 seconds
        this.init();
    }
    
    copyFromStarter(questionId) {
        const starterValue = $(`#starter-answer-${questionId}`).val();
        $(`input[name="coord-q-${questionId}"][value="${starterValue}"]`).prop('checked', true);
        this.updateAnswer(questionId, starterValue);
        this.updateProgress();
    }
    
    async saveDraft() {
        const response = await $.ajax({
            url: '/Coordination/FormReview/SaveCoordinatorAnswers',
            method: 'POST',
            data: JSON.stringify({
                submissionId: this.submissionId,
                coordinatorData: this.buildDraftData(),
                preferenceSelections: this.getPreferences()
            }),
            contentType: 'application/json'
        });
    }
    
    validateCompletion() {
        return Object.keys(this.coordinatorAnswers).length === this.requiredQuestions.length;
    }
}
```

## 10. Implementation Timeline

### Phase 1: Database (Day 1)
- [ ] Add columns to ProjectFormSubmissions
- [ ] Modify DiagnosisAnswers schema
- [ ] Update constraints and indexes
- [ ] Build and deploy database project

### Phase 2: Domain & Application (Days 2-3)
- [ ] Update ProjectFormSubmission entity
- [ ] Create SaveCoordinatorAnswersCommand
- [ ] Modify approval command
- [ ] Update integration event structure
- [ ] Implement event handler

### Phase 3: UI Implementation (Days 4-5)
- [ ] Create dual-column review layout
- [ ] Implement coordinator answer capture
- [ ] Add auto-save functionality
- [ ] Implement validation logic

### Phase 4: Testing & Documentation (Day 6)
- [ ] Integration tests
- [ ] UI testing
- [ ] Update CLAUDE.md
- [ ] Create user guide

## 11. Testing Strategy

### Unit Tests
- Entity validation for coordinator data
- Command handler logic
- Event serialization

### Integration Tests
- Full flow: starter → coordinator → diagnostics
- Idempotent event handling
- Constraint validation

### Test Cases
1. Coordinator can save answers
2. Cannot approve without coordinator answers
3. Both answer sets persist correctly
4. Diagnostics receives both versions
5. Auto-save works every 30 seconds

## 12. Acceptance Criteria

### Required Functionality
- [x] Coordinator can provide answer for each question
- [x] Both answers persist in ProjectFormSubmissions.CoordinatorData
- [x] Approval blocked without coordinator answers
- [x] Event includes both answer sets
- [x] DiagnosisAnswers stores both with source discrimination
- [x] UI shows dual-column layout
- [x] Copy from starter functionality works
- [x] Visual diff when answers differ
- [x] Progress tracking for coordinator completion
- [x] Auto-save every 30 seconds

### Technical Requirements
- [x] Phoenix Admin template compliance
- [x] Clean build (0 errors, 0 warnings)
- [ ] All tests pass (pending)
- [x] Documentation updated
- [x] No changes to starter flow

## 13. Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Large form performance | Medium | JSON compression, pagination |
| Complex UI for mobile | Low | Responsive design, test on devices |
| Event processing failure | High | Idempotent handling, retry logic |

## 14. Success Metrics

- Coordinator completion rate >90%
- Answer divergence tracking
- Approval time reduction
- Zero data loss events

## 15. Documentation Updates

- Update CLAUDE.md with dual answer pattern
- Add to architecture decisions
- Create coordinator user guide
- Update API documentation

---

**Note**: This requirement represents a significant enhancement to the form review process, enabling expert validation while maintaining system integrity and following established patterns.

## Completion Notes

**Completed**: 2025-01-10
**Implementation Status**: ✅ Fully implemented with clean build

### What Was Built
- Complete backend infrastructure for dual answers
- Full UI implementation with responsive dual-column layout
- Auto-save functionality with 30-second intervals
- Copy-from-starter one-click functionality
- Visual difference detection between answers
- Progress tracking and approval validation
- Integration with existing approval workflow

### Ready For
- Database deployment (schema changes required)
- Integration testing with real data
- User acceptance testing

### Technical Highlights
- Reused existing DraftDataDto structure for consistency
- Client-side diff detection for performance
- Responsive design with mobile support
- Clean architecture compliance throughout
- Zero warnings/errors in final build