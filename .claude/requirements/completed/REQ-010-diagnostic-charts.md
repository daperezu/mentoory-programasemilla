# REQ-010: Diagnostic Charts from Approved Forms

> **Priority**: P1  
> **Module**: Diagnostics  
> **Estimate**: Large  
> **Status**: Pending  
> **Branch**: feature/diagnostic-charts  

## Summary
Generate radial charts per block from diagnosis answers after form approval, with coordinator-preferred scoring, modern UI visualization, and print-ready output for coordinator review.

## Business Context
After coordinators approve participant forms with their own responses, the system needs to visualize diagnostic scores as radial charts for analysis. The coordinator's answers can override starter answers for diagnosis purposes, enabling more accurate assessments. Charts must be viewable on-screen and printable for offline review meetings.

## Acceptance Criteria
- [ ] System generates one radial chart per block from approved form data
- [ ] Chart points labeled as `{blockId}.{internalQuestionId}` (e.g., "6.2", "8.4")
- [ ] Coordinator answers override starter answers when `PreferredForDiagnosis = true`
- [ ] Multi-select questions aggregate scores (sum by default)
- [ ] Charts display in modern, centered, vertically-stacked layout
- [ ] Print view shows incubator/project/participant header with clean chart layout
- [ ] Charts load once (no real-time updates) and are cacheable
- [ ] Only coordinators can access the review screen
- [ ] Charts use Phoenix Admin Template's ECharts integration (no new libraries)

## Technical Requirements

### Domain Layer

**New Services:**
- `DiagnosisScoreCalculator` - Domain service for score aggregation logic
  - `CalculateQuestionScore(IEnumerable<DiagnosisAnswer> answers)` 
  - `DeterminePreferredSource(IEnumerable<DiagnosisAnswer> answers)`
  - `AggregateMultiSelectScores(IEnumerable<DiagnosisAnswer> answers)`

**New Value Objects:**
- `QuestionScore` - Encapsulates final score for a question
  ```csharp
  public class QuestionScore : ValueObject
  {
      public long QuestionId { get; }
      public string Label { get; } // "blockId.internalId"
      public decimal Score { get; }
      public string Source { get; } // "Starter" or "Coordinator"
  }
  ```

- `BlockChartData` - Data for one radial chart
  ```csharp
  public class BlockChartData : ValueObject
  {
      public long BlockId { get; }
      public string BlockName { get; }
      public IReadOnlyList<QuestionScore> Scores { get; }
      public decimal MaxScore { get; }
  }
  ```

**Repository Extensions:**
- `IUserProjectDiagnosisRepository` additions:
  - `GetApprovedDiagnosisAnswersAsync(long projectId, string userId, QuestionPhase phase)`
  - `GetBlocksWithQuestionsAsync(long projectId)`

**Domain Rules:**
- If any coordinator answer has `PreferredForDiagnosis = true` for a question, use ONLY coordinator answers
- Otherwise, use starter answers by default
- Multi-select scores aggregate via sum unless configured otherwise
- Scores must be numeric and non-negative

### Application Layer

**Queries:**
- `GetDiagnosisChartDataQuery` - Fetches and aggregates chart data
  ```csharp
  public record GetDiagnosisChartDataQuery(
      long ProjectId,
      string ParticipantUserId,
      QuestionPhase Phase) : IBaseRequest<DiagnosisReviewDto>;
  ```

**DTOs:**
```csharp
public class DiagnosisReviewDto
{
    public string IncubatorName { get; set; }
    public string ProjectName { get; set; }
    public string ParticipantName { get; set; }
    public DateTime ApprovalDate { get; set; }
    public QuestionPhase Phase { get; set; }
    public List<DiagnosisChartDto> Charts { get; set; }
}

public class DiagnosisChartDto
{
    public long BlockId { get; set; }
    public string BlockName { get; set; }
    public List<QuestionScoreDto> Scores { get; set; }
    public decimal MaxScore { get; set; }
}

public class QuestionScoreDto
{
    public string Label { get; set; } // "6.2"
    public decimal Value { get; set; }
    public string QuestionText { get; set; }
    public string Source { get; set; } // For audit
}
```

**Query Handler Logic:**
```csharp
public class GetDiagnosisChartDataQueryHandler : BaseQueryHandler<GetDiagnosisChartDataQuery, DiagnosisReviewDto>
{
    // 1. Verify coordinator access
    // 2. Fetch all diagnosis answers for project/user/phase
    // 3. Group by BlockId
    // 4. For each block:
    //    a. Group answers by QuestionId
    //    b. Apply preference logic (coordinator override)
    //    c. Aggregate scores (sum for multi-select)
    //    d. Create chart data with labels
    // 5. Return complete review DTO
}
```

### Infrastructure Layer

**Repository Implementation:**
```csharp
public async Task<IEnumerable<DiagnosisAnswer>> GetApprovedDiagnosisAnswersAsync(
    long projectId, string userId, QuestionPhase phase, CancellationToken cancellationToken)
{
    return await dbContext.DiagnosisAnswers
        .Where(a => a.ProjectId == projectId 
                 && a.UserId == userId 
                 && a.Phase == (int)phase
                 && a.IsUsedForDiagnosis)
        .OrderBy(a => a.BlockId)
        .ThenBy(a => a.Order)
        .ToListAsync(cancellationToken);
}
```

**Caching Service:**
```csharp
public interface IDiagnosisChartCacheService
{
    Task<DiagnosisReviewDto?> GetCachedChartDataAsync(string cacheKey);
    Task SetCachedChartDataAsync(string cacheKey, DiagnosisReviewDto data, TimeSpan expiration);
    string GenerateCacheKey(long projectId, string userId, QuestionPhase phase);
}
```

### Web Layer

**Controller:**
```csharp
[Area("Diagnostics")]
[Authorize(Roles = Roles.Coordinator)]
public class DiagnosisChartsController : BaseController
{
    [HttpGet("Review/{projectId:long}/{participantUserId}/{phase:int}")]
    public async Task<IActionResult> Review(long projectId, string participantUserId, QuestionPhase phase)
    {
        // Verify coordinator access to project
        // Execute query to get chart data
        // Map to view model
        // Return view
    }
    
    [HttpGet("Print/{projectId:long}/{participantUserId}/{phase:int}")]
    public async Task<IActionResult> Print(long projectId, string participantUserId, QuestionPhase phase)
    {
        // Same as Review but with print-specific layout
    }
}
```

**View Models:**
```csharp
public class DiagnosisReviewViewModel
{
    public string IncubatorName { get; set; }
    public string ProjectName { get; set; }
    public string ParticipantName { get; set; }
    public string PhaseDisplay { get; set; }
    public DateTime ReviewDate { get; set; }
    public List<ChartViewModel> Charts { get; set; }
    public string PrintUrl { get; set; }
}

public class ChartViewModel
{
    public string BlockId { get; set; }
    public string BlockName { get; set; }
    public string ChartElementId { get; set; }
    public string ChartDataJson { get; set; } // Pre-serialized for JS
}
```

**JavaScript Module (`diagnosis-charts.js`):**
```javascript
class DiagnosisChartsManager {
    constructor() {
        this.charts = new Map();
        this.initializeCharts();
    }
    
    initializeCharts() {
        document.querySelectorAll('[data-diagnosis-chart]').forEach(element => {
            const chartId = element.dataset.diagnosisChart;
            const chartData = JSON.parse(element.dataset.chartData);
            this.renderRadarChart(element, chartData);
        });
    }
    
    renderRadarChart(container, data) {
        const chart = echarts.init(container);
        const option = {
            title: { text: data.blockName, left: 'center' },
            radar: {
                indicator: data.labels.map(label => ({
                    name: label,
                    max: data.maxScore
                })),
                shape: 'circle',
                splitNumber: 5,
                axisName: {
                    color: '#666',
                    fontSize: 12
                }
            },
            series: [{
                type: 'radar',
                data: [{
                    value: data.scores,
                    name: data.blockName,
                    areaStyle: {
                        opacity: 0.3,
                        color: 'rgba(34, 126, 230, 0.5)'
                    },
                    lineStyle: {
                        width: 2,
                        color: '#227ee6'
                    }
                }]
            }],
            tooltip: {
                trigger: 'item',
                formatter: (params) => {
                    // Show question text and score on hover
                }
            }
        };
        chart.setOption(option);
        this.charts.set(chartId, chart);
    }
    
    printView() {
        window.print();
    }
}
```

## Database Changes

**Schema Modifications:**
```sql
-- Add InternalQuestionId column if not exists
ALTER TABLE [diagnostics].[DiagnosisAnswers]
ADD [InternalQuestionId] INT NULL;

-- Add composite index for efficient querying
CREATE NONCLUSTERED INDEX [IX_DiagnosisAnswers_Project_User_Phase_Block]
ON [diagnostics].[DiagnosisAnswers] (
    [ProjectId] ASC,
    [UserId] ASC,
    [Phase] ASC,
    [BlockId] ASC
) INCLUDE ([QuestionId], [Score], [AnswerSource], [PreferredForDiagnosis]);

-- Consider materialized view for aggregated scores
CREATE VIEW [diagnostics].[vw_AggregatedDiagnosisScores]
WITH SCHEMABINDING
AS
SELECT 
    ProjectId,
    UserId,
    Phase,
    BlockId,
    QuestionId,
    SUM(Score) as TotalScore,
    COUNT_BIG(*) as AnswerCount
FROM [diagnostics].[DiagnosisAnswers]
WHERE IsUsedForDiagnosis = 1
GROUP BY ProjectId, UserId, Phase, BlockId, QuestionId;
```

## UI/UX Requirements

**Screen Layout:**
```
┌─────────────────────────────────────────┐
│  [Logo] Diagnóstico de Proyecto        │
│  ────────────────────────────────────   │
│  Incubadora: [Name]                     │
│  Proyecto: [Name]                       │
│  Participante: [Name]                   │
│  Fase: [Inicio/Final]                   │
│  ────────────────────────────────────   │
│                                         │
│  ┌───────────────────────────────┐     │
│  │     Bloque 1: [Name]          │     │
│  │     [Radar Chart]             │     │
│  └───────────────────────────────┘     │
│                                         │
│  ┌───────────────────────────────┐     │
│  │     Bloque 2: [Name]          │     │
│  │     [Radar Chart]             │     │
│  └───────────────────────────────┘     │
│                                         │
│  [Print Button]  [Export Button]        │
└─────────────────────────────────────────┘
```

**Print Styles (`diagnosis-print.css`):**
```css
@media print {
    /* Hide navigation and controls */
    .navbar, .sidebar, .footer, .btn-print { display: none !important; }
    
    /* Page setup */
    @page {
        size: A4 landscape;
        margin: 2cm;
    }
    
    /* Chart sizing */
    .diagnosis-chart {
        page-break-inside: avoid;
        height: 400px;
        width: 100%;
    }
    
    /* Header styling */
    .print-header {
        border-bottom: 2px solid #333;
        padding-bottom: 1rem;
        margin-bottom: 2rem;
    }
    
    /* Force colors */
    .radar-chart * {
        -webkit-print-color-adjust: exact;
        print-color-adjust: exact;
    }
}
```

## Dependencies
- [ ] Depends on: REQ-008 (Dual Answers System) - Must be completed first
- [ ] Depends on: DiagnosisAnswers table populated with approved form data
- [ ] Requires: Phoenix Admin Template with ECharts library available
- [ ] External: None

## Testing Requirements

**Unit Tests:**
- `DiagnosisScoreCalculator` aggregation logic
- Preference determination (coordinator vs starter)
- Multi-select score summation
- Label generation (`blockId.internalId`)

**Integration Tests:**
- Repository query performance with large datasets
- Cache service operations
- End-to-end chart data generation

**UI Tests:**
- Chart rendering with various data sets
- Print preview functionality
- Responsive layout on different screen sizes

**Performance Tests:**
- Query optimization for 1000+ answers
- Chart rendering with 20+ blocks
- Cache hit/miss scenarios

## Security Considerations
- **Authentication**: User must be authenticated
- **Authorization**: Only coordinators can access review screens
- **Project Access**: Verify coordinator has access to specific project
- **Data Protection**: No PII in chart labels, only IDs
- **Input Validation**: Validate projectId, userId, phase parameters
- **XSS Prevention**: Sanitize all text displayed in charts

## Documentation Updates
- [ ] Update `.claude/architecture.md` with chart visualization pattern
- [ ] Add to `.claude/domain-reference.md` for score aggregation logic
- [ ] Document caching strategy in `.claude/common-issues.md`
- [ ] Add print CSS patterns to `.claude/web-patterns.md`
- [ ] Create user guide for coordinator chart review

## Implementation Notes

**Critical Considerations:**
1. **Performance**: Pre-aggregate scores at query level, not in application code
2. **Caching**: Cache is safe because approved data is immutable
3. **Chart Library**: Must use existing ECharts from Phoenix Admin Template
4. **Label Format**: Strictly follow `blockId.internalQuestionId` format
5. **Score Aggregation**: Default to SUM for multi-select unless specified otherwise
6. **Print Quality**: Test on actual printers, not just print preview

**Edge Cases:**
- Handle questions with no answers (score = 0)
- Handle blocks with single question (still show radar chart)
- Handle missing coordinator preferences (fallback to starter)
- Handle very long block names (truncate in chart)

## Definition of Done
- [ ] Code implemented following DDD patterns
- [ ] All tests written and passing (unit, integration, UI)
- [ ] StyleCop compliance verified (zero warnings)
- [ ] Charts render correctly in Chrome, Firefox, Edge
- [ ] Print output verified on physical printer
- [ ] Performance acceptable for 1000+ answers
- [ ] Caching working with 5-minute TTL
- [ ] Documentation updated in all specified locations
- [ ] Code reviewed by senior developer
- [ ] Deployed to staging environment
- [ ] Acceptance criteria verified by product owner

## Follow-up Tasks
- **Phase 2**: Export charts to PDF programmatically
- **Phase 3**: Comparative analysis between multiple participants
- **Phase 4**: Historical trend charts across phases
- **Phase 5**: Aggregate charts at project level
- **Enhancement**: Configurable aggregation methods (avg, median, etc.)
- **Enhancement**: Interactive chart drill-down to see question details
- **Enhancement**: Color-coding based on score thresholds

---

**Note**: This requirement builds upon the dual-answer system (REQ-008) and assumes that coordinator answers with preference flags are already being captured in the DiagnosisAnswers table. The implementation should be fully compatible with the existing Phoenix Admin Template and maintain strict DDD boundaries.