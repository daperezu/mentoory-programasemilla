# Work Log

## 2025-01-14 - Public Homepage Geolocation Architecture

### Context
Analyzed Cloudy Code (Elixir/Phoenix) requirements for a public homepage with geolocation features. Adapted design for LinaSys ASP.NET Core architecture with focus on Azure SQL Database optimization.

### Completed

1. **Requirements Documentation**:
   - Created `REQ-011-public-homepage-geolocation.md` in pending folder
   - Comprehensive 1000+ line specification
   - Adapted from Elixir/Phoenix to ASP.NET Core patterns
   - Added SSDT database structure (not migrations)

2. **Architecture Decision Record**:
   - Created `ADR-002-geolocation-architecture.md`
   - Initially designed dual-strategy (Spatial vs Geohash)
   - **Chose Geohash strategy** for Azure optimization
   - Updated all docs to single Geohash approach

3. **Technical Analysis**:
   - Created `geolocation-technical-analysis.md`
   - Performance benchmarks: Geohash uses 10-50 DTUs vs 100-500 for spatial
   - Cost analysis: 80-90% reduction in Azure SQL costs
   - Accuracy trade-off: ±1% error acceptable for 15km searches

4. **Database Implementation Guide**:
   - Created `REQ-011-database-implementation.md`
   - SSDT structure (one file per object)
   - No migrations - direct schema changes

### Key Decisions

**Why Geohash Over SQL Server Spatial**:
```csharp
// Rejected: SQL Server Geography
[GeoLocation] AS geography::Point([Latitude], [Longitude], 4326)
// High Azure DTU cost, requires Premium tier

// Chosen: Geohash with B-tree indexes
[Geohash] VARCHAR(12),
[GeohashPrefix5] AS LEFT([Geohash], 5) PERSISTED
// 90% cost reduction, works on all tiers
```

**Implementation Pattern**:
```csharp
// Step 1: Query with geohash + bounding box
var candidates = await _context.Projects
    .Where(p => neighbors.Contains(p.GeohashPrefix5))
    .Where(p => p.Latitude BETWEEN bounds.MinLat AND bounds.MaxLat)
    .ToListAsync();

// Step 2: Haversine in application layer
var nearby = candidates
    .Where(c => HaversineCalculator.DistanceInKm(...) <= radiusKm)
    .OrderBy(x => x.Distance);
```

### Files Created/Modified
- `.claude/requirements/pending/REQ-011-public-homepage-geolocation.md`
- `.claude/requirements/analysis/geolocation-technical-analysis.md`
- `.claude/requirements/analysis/REQ-011-database-implementation.md`
- `.claude/architecture-decisions/ADR-002-geolocation-architecture.md`
- `CLAUDE.md` - Added REQ-011 to pending list

### Problems & Solutions

**Problem**: Azure SQL spatial queries consume excessive DTUs
**Solution**: Geohash indexing with application-layer distance calculation

**Problem**: SSDT vs migration scripts confusion
**Solution**: Clarified LinaSys uses SSDT (not in production yet)

### Next Implementation Steps
1. Add Geohash columns to Projects table
2. Create B-tree indexes on GeohashPrefix5/6
3. Implement GeoCoordinate value object
4. Build HaversineCalculator utility
5. Create GetNearbyProjectsQuery with dual filtering

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