# REQ-012 Implementation Plan

## Phase-by-Phase Execution Plan

### PHASE 1: Backend Enhancements [2-3 hours]
**Objective**: Enable dual discovery modes (time-based and location-based)

#### Step 1.1: Create Time-Based Query
```csharp
// GetLatestProjectsQuery.cs
public record GetLatestProjectsQuery(
    int MaxResults = 10,
    bool IncludeStages = true
) : IBaseRequest<LatestProjectsDto>;
```

#### Step 1.2: Create Query Handler
- Load projects with stages
- Filter by active status
- Sort by nearest StartDate
- Include business incubator information
- Map to DTOs with stage information

#### Step 1.3: Update Controller
```csharp
[HttpGet]
public async Task<IActionResult> GetLatestProjects()
{
    var query = new GetLatestProjectsQuery();
    var result = await _mediatorExecutor.SendAndLogIfFailureAsync(query);
    return Json(result.Value);
}
```

#### Step 1.4: Database Seeds
- Create 15-20 sample projects
- Varied start dates (next 60 days)
- Mix with/without geolocation
- Add realistic ProjectStages
- Include hero image references

**Checkpoint**: Backend supports both discovery modes ✅

---

### PHASE 2: Phoenix Layout Integration [2 hours]
**Objective**: Integrate Phoenix theme while preserving functionality

#### Step 2.1: Update _PublicLayout.cshtml
- Copy Phoenix navbar structure
- Add theme toggle component
- Update footer design
- Maintain authentication links
- Add Phoenix CSS/JS references

#### Step 2.2: Create Phoenix Overrides
```css
/* public-phoenix.css */
:root {
    --phoenix-primary: #667eea;
    --phoenix-gradient: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.hero-gradient {
    background: var(--phoenix-gradient);
}
```

#### Step 2.3: Initialize Phoenix Components
```javascript
// phoenix-init.js
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Phoenix tooltips
    const tooltipTriggerList = [].slice.call(
        document.querySelectorAll('[data-bs-toggle="tooltip"]')
    );
    tooltipTriggerList.map(el => new bootstrap.Tooltip(el));

    // Theme toggle
    initializeThemeToggle();
});
```

**Checkpoint**: Phoenix theme working with existing pages ✅

---

### PHASE 3: Homepage Redesign [3-4 hours]
**Objective**: Implement dual-mode discovery with Phoenix components

#### Step 3.1: Hero Section Structure
```html
<section class="hero-gradient text-white py-5">
    <div class="container">
        <div class="row align-items-center min-vh-50">
            <div class="col-lg-6">
                <h1 class="display-4 fw-bold">Descubre Proyectos de Emprendimiento</h1>
                <p class="lead">Conecta con emprendedores innovadores</p>
                <div class="d-flex gap-3">
                    <button class="btn btn-light btn-lg" id="btnExploreProjects">
                        <i class="fas fa-compass"></i> Explorar Proyectos
                    </button>
                    <button class="btn btn-outline-light btn-lg" id="btnUseLocation">
                        <i class="fas fa-location-arrow"></i> Usar mi Ubicación
                    </button>
                </div>
            </div>
            <div class="col-lg-6">
                <!-- Hero illustration or pattern -->
            </div>
        </div>
    </div>
</section>
```

#### Step 3.2: Value Props Section
```html
<section class="py-5">
    <div class="container">
        <div class="row g-4">
            @foreach (var feature in Model.Features)
            {
                <div class="col-md-6 col-lg-4">
                    <div class="card h-100 card-hover">
                        <div class="card-body text-center">
                            <div class="icon-circle bg-primary-subtle mb-3">
                                <i class="@feature.Icon fa-2x text-primary"></i>
                            </div>
                            <h5>@feature.Title</h5>
                            <p class="text-muted">@feature.Description</p>
                        </div>
                    </div>
                </div>
            }
        </div>
    </div>
</section>
```

#### Step 3.3: Project Discovery Sections
```html
<!-- Default: Time-based -->
<section id="latestProjects" class="py-5 bg-light">
    <div class="container">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>Próximos Proyectos</h2>
            <span class="badge bg-info">
                <i class="fas fa-clock"></i> Ordenado por: Fecha de inicio
            </span>
        </div>
        <div class="row g-4" id="latestProjectsGrid">
            <!-- Project cards rendered here -->
        </div>
    </div>
</section>

<!-- Enhanced: Location-based (hidden by default) -->
<section id="nearbyProjects" class="py-5 bg-light d-none">
    <div class="container">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>Proyectos Cercanos</h2>
            <span class="badge bg-success">
                <i class="fas fa-map-marker-alt"></i> Ordenado por: Cercanía
            </span>
        </div>
        <div class="row g-4" id="nearbyProjectsGrid">
            <!-- Project cards with distance badges -->
        </div>
    </div>
</section>
```

#### Step 3.4: JavaScript Mode Management
```javascript
// public-projects.js updates
let discoveryMode = 'time'; // 'time' or 'location'

function initializeDiscovery() {
    // Load time-based projects by default
    loadLatestProjects();

    // Setup location button
    $('#btnUseLocation').on('click', function() {
        if (navigator.geolocation) {
            enableLocationMode();
        } else {
            showToast('Tu navegador no soporta geolocalización', 'warning');
        }
    });
}

function loadLatestProjects() {
    $.get('/Public/Projects/GetLatestProjects')
        .done(function(data) {
            renderProjectCards(data.projects, '#latestProjectsGrid', false);
        });
}

function enableLocationMode() {
    navigator.geolocation.getCurrentPosition(
        function(position) {
            discoveryMode = 'location';
            loadNearbyProjects(position.coords);
            toggleSections();
        },
        function(error) {
            showLocationError(error);
        }
    );
}

function toggleSections() {
    if (discoveryMode === 'location') {
        $('#latestProjects').addClass('d-none');
        $('#nearbyProjects').removeClass('d-none');
    } else {
        $('#latestProjects').removeClass('d-none');
        $('#nearbyProjects').addClass('d-none');
    }
}
```

**Checkpoint**: Homepage with dual discovery modes working ✅

---

### PHASE 4: Project Details Page [2-3 hours]
**Objective**: Create Phoenix-styled project detail view

#### Step 4.1: Details View Structure
```html
@model ProjectDetailViewModel
@{
    Layout = "_PublicLayout";
    ViewData["Title"] = Model.Name;
}

<!-- Hero Section -->
<section class="position-relative">
    <div class="hero-image-container">
        <img src="@Model.HeroImageUrl" class="w-100" style="height: 400px; object-fit: cover;">
        <div class="hero-overlay">
            <div class="container">
                <h1 class="display-4 text-white">@Model.Name</h1>
                <p class="lead text-white">@Model.BusinessIncubatorName</p>
            </div>
        </div>
    </div>
</section>

<!-- Metadata Chips -->
<section class="py-3 border-bottom">
    <div class="container">
        <div class="d-flex flex-wrap gap-2">
            <span class="badge bg-primary">
                <i class="fas fa-calendar"></i> @Model.StartDate.ToString("dd MMM yyyy")
            </span>
            <span class="badge bg-info">
                <i class="fas fa-map-marker-alt"></i> @Model.LocationName
            </span>
            <span class="badge bg-success">
                <i class="fas fa-users"></i> @Model.ActiveParticipants participantes
            </span>
            @if (Model.CurrentStage != null)
            {
                <span class="badge bg-warning">
                    <i class="fas fa-flag"></i> Etapa: @Model.CurrentStage
                </span>
            }
        </div>
    </div>
</section>

<!-- Main Content -->
<section class="py-5">
    <div class="container">
        <div class="row">
            <div class="col-lg-8">
                <!-- Description -->
                <div class="card mb-4">
                    <div class="card-body">
                        <h3>Acerca del Proyecto</h3>
                        <p>@Model.Description</p>
                    </div>
                </div>

                <!-- Stages Timeline -->
                @if (Model.Stages.Any())
                {
                    <div class="card mb-4">
                        <div class="card-body">
                            <h3>Cronograma</h3>
                            <div class="timeline">
                                @foreach (var stage in Model.Stages)
                                {
                                    <div class="timeline-item @(stage.IsActive ? "active" : "")">
                                        <div class="timeline-icon">
                                            <i class="fas fa-check"></i>
                                        </div>
                                        <div class="timeline-content">
                                            <h5>@stage.Title</h5>
                                            <p class="text-muted">
                                                @stage.StartDate.ToString("dd MMM") - @stage.EndDate.ToString("dd MMM yyyy")
                                            </p>
                                            <p>@stage.Description</p>
                                        </div>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>
                }
            </div>

            <div class="col-lg-4">
                <!-- Organizer Card -->
                <div class="card mb-4">
                    <div class="card-body">
                        <h5>Organizador</h5>
                        <div class="d-flex align-items-center mb-3">
                            <div class="avatar avatar-xl">
                                <img src="/img/incubator-placeholder.png" class="rounded-circle">
                            </div>
                            <div class="ms-3">
                                <h6 class="mb-0">@Model.BusinessIncubatorName</h6>
                                <small class="text-muted">Incubadora de Negocios</small>
                            </div>
                        </div>
                        <p class="small">@Model.BusinessIncubatorDescription</p>
                    </div>
                </div>

                <!-- CTA Card -->
                <div class="card">
                    <div class="card-body text-center">
                        <h5>¿Interesado en este proyecto?</h5>
                        <p class="text-muted">Registra tu interés y te contactaremos</p>
                        <button class="btn btn-primary w-100" id="btnRegisterInterest">
                            <i class="fas fa-heart"></i> Registrar Interés
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</section>
```

#### Step 4.2: Details Query Implementation
```csharp
public class GetProjectDetailsQueryHandler : BaseCommandHandler<GetProjectDetailsQuery, ProjectDetailDto>
{
    public override async Task<Result<ProjectDetailDto>> Handle(
        GetProjectDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetProjectWithDetailsAsync(
            request.ExternalId,
            cancellationToken);

        if (project == null)
            return Failure(ResultErrorCodes.Project_NotFound,
                ("Project", "Proyecto no encontrado"));

        var dto = MapToDetailDto(project);
        return Success(dto);
    }
}
```

**Checkpoint**: Details page complete with Phoenix styling ✅

---

### PHASE 5: Testing & Polish [2 hours]
**Objective**: Ensure quality and performance

#### Step 5.1: Functional Testing
- [ ] Homepage loads without location
- [ ] Latest projects display correctly
- [ ] Location button works
- [ ] Nearby projects load with distance
- [ ] Navigation to details works
- [ ] Interest registration works
- [ ] Spanish translations correct

#### Step 5.2: Responsive Testing
- [ ] Mobile (360px - 767px)
- [ ] Tablet (768px - 1023px)
- [ ] Desktop (1024px+)
- [ ] All Phoenix components responsive

#### Step 5.3: Performance Checks
- [ ] Lazy load images
- [ ] Minimize JavaScript
- [ ] Bundle CSS
- [ ] Test with 20+ projects

#### Step 5.4: Build Validation
```bash
dotnet build
# Must show: 0 Warning(s), 0 Error(s)

cd Db
MSBuild LinaDb.sqlproj -p:Configuration=Debug
# Must complete successfully

.\Publish-LinaDb.ps1 -Publish
# Must deploy without errors
```

**Checkpoint**: Solution ready for deployment ✅

---

## Resume Points

### After Phase 1 Completion
```yaml
Status: Backend ready
Next: Start Phase 2 - Update _PublicLayout.cshtml
Context: Time-based query working, seed data added
```

### After Phase 2 Completion
```yaml
Status: Phoenix theme integrated
Next: Start Phase 3 - Redesign homepage sections
Context: Layout updated, Phoenix CSS/JS ready
```

### After Phase 3 Completion
```yaml
Status: Homepage redesigned
Next: Start Phase 4 - Create details page
Context: Dual discovery modes working
```

### After Phase 4 Completion
```yaml
Status: All views complete
Next: Start Phase 5 - Testing
Context: Details page done, interest tracking ready
```

## Critical Path Items
1. Maintain existing geolocation functionality
2. Ensure time-based discovery as default
3. Keep all text in Spanish
4. Follow StyleCop rules (zero warnings)
5. Test both discovery modes thoroughly