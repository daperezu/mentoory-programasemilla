# Work Log

## 2025-01-11 - Diagnostic Charts Requirements & Planning

### Context
Analyzed requirements prompt for generating diagnostic charts from approved forms. Created comprehensive implementation plan for visualizing diagnosis scores as radial charts per block.

### Completed

1. **Requirements Analysis**:
   - Analyzed prompt in `.claude/requirements/prompts/linasys_diagnosis_charts_prompt.md`
   - Translated Elixir/Phoenix requirements to ASP.NET Core/C# architecture
   - Identified existing infrastructure (ECharts in Phoenix Admin Template)

2. **Domain Exploration**:
   - Reviewed `DiagnosisAnswer` entity structure
   - Examined `DiagnosisAnswers` table schema
   - Found existing columns: `AnswerSource`, `PreferredForDiagnosis`, `Score`

3. **Requirements Document Created**:
   - Created `REQ-010-diagnostic-charts.md` following LinaSys template
   - Saved to `.claude/requirements/pending/`
   - Document approved by user for implementation

### Key Decisions

**Architecture Approach**:
- Use existing ECharts library (no new dependencies)
- Implement domain service for score aggregation
- Cache results (data immutable post-approval)

**Score Aggregation Logic**:
```csharp
// Pseudo-code for preference logic
if (answers.Any(a => a.AnswerSource == "Coordinator" && a.PreferredForDiagnosis))
    useCoordinatorAnswers();
else
    useStarterAnswers();
```

**Chart Configuration**:
- One radial/radar chart per block
- Labels: `{blockId}.{internalQuestionId}` format
- Multi-select: SUM aggregation by default

### Technical Specifications

**New Components**:
- `DiagnosisScoreCalculator` (Domain Service)
- `GetDiagnosisChartDataQuery` (Application Query)
- `DiagnosisChartsController` (Web Controller)
- `diagnosis-charts.js` (JavaScript module)
- `diagnosis-print.css` (Print styles)

**Database Enhancement**:
- Add `InternalQuestionId` column to `DiagnosisAnswers`
- Create composite index for performance
- Consider materialized view for aggregations

### Files Created
- `.claude/requirements/pending/REQ-010-diagnostic-charts.md`

### Next Steps
1. Implement domain services for score calculation
2. Create application queries and DTOs
3. Build coordinator review UI
4. Integrate ECharts for visualization
5. Add print-ready CSS