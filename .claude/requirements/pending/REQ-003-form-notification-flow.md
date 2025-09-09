# REQ-003: Project-Scoped Dashboard with Form Discovery

## Status: Pending
## Created: 2025-09-07
## Priority: High

## Problem Statement
The current system has two major issues:
1. **Dashboard shows ALL projects**: Starters select a specific project in context selector, but dashboard shows all their projects instead of focusing on the selected one
2. **Forms are hidden until created**: Users cannot see available forms based on active stages - they only see forms that already exist in the database

This creates confusion and prevents users from discovering forms they need to complete.

## Root Cause Analysis

### Current Implementation Issues
1. **Dashboard ignores selected project context**: `GetParticipantProjectsQuery` returns all projects for the user
2. **GetPendingFormsQuery** only returns existing `ProjectFormSubmissions` with Draft/Submitted status
3. Forms are created lazily via `GetOrCreateFormSubmissionCommand` when accessed through form editor
4. Dashboard has no way to show "available but not yet created" forms
5. No notifications when stages become active
6. Users must somehow know to navigate to `/ProjectFormSubmission/Edit/{id}` to trigger form creation

### Context Selection System
- Starters already select a project in `ContextSelectionController`
- Selected context is stored and available via `CurrentUserContext.ProjectId`
- Dashboard doesn't use this selected project, showing all projects instead

## Desired Behavior
1. **Project-Scoped Dashboard**: Show only the selected project's information
2. **Form Discovery**: Display ALL available forms (created or not) based on active stages
3. **Clear Actions**: "Start Form" for new forms, "Continue" for drafts, "View" for submitted
4. **Notifications**: Email when stages activate with direct link to dashboard
5. **Maintains Lazy Creation**: Forms created on-demand when user clicks "Start Form"

## Implementation Proposal

### Phase 1: Project-Scoped Dashboard

#### 1.1 Update DashboardController
```csharp
[HttpGet]
public async Task<IActionResult> Index()
{
    // Require project context for Starters
    var context = DemandCurrentUserContext(requireProject: true, 
        errorMessage: "Debe seleccionar un proyecto para ver el panel de control");
    
    var userId = CurrentUserId;
    var projectId = context.ProjectId!.Value;
    
    // Get single project details instead of all projects
    var projectQuery = new GetProjectDetailsQuery(projectId);
    var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);
    
    // Get available forms for this specific project
    var availableFormsQuery = new GetAvailableFormsQuery(userId, projectId);
    var availableFormsResult = await MediatorExecutor.SendAndLogIfFailureAsync(availableFormsQuery);
    
    // Get project-specific activities
    var activitiesQuery = new GetProjectActivitiesQuery(userId, projectId, 10);
    var activitiesResult = await MediatorExecutor.SendAndLogIfFailureAsync(activitiesQuery);
    
    var viewModel = new ProjectDashboardViewModel
    {
        UserName = User.Identity?.Name ?? "Usuario",
        Project = MapProjectToViewModel(projectResult),
        AvailableForms = MapAvailableFormsToViewModel(availableFormsResult),
        RecentActivities = MapActivitiesToViewModel(activitiesResult),
        SelectedProjectName = projectResult.Value?.Name ?? "Proyecto"
    };
    
    return View(viewModel);
}

[HttpGet("Forms/Start")]
public async Task<IActionResult> StartForm(QuestionPhase phase)
{
    var context = DemandCurrentUserContext(requireProject: true);
    
    // Get project external ID for the command
    var projectQuery = new GetProjectByIdQuery(context.ProjectId!.Value);
    var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);
    
    if (!projectResult.IsSuccess || projectResult.Value == null)
    {
        this.SetErrorToast("Proyecto no encontrado");
        return RedirectToAction("Index");
    }
    
    // Trigger lazy form creation
    var command = new GetOrCreateFormSubmissionCommand(
        projectResult.Value.ExternalId, 
        CurrentUserId, 
        phase);
    var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
    
    if (result.IsSuccess)
    {
        // Redirect to form editor
        return RedirectToAction("Edit", "ProjectFormSubmission", 
            new { area = "BusinessIncubators", id = result.Value.ExternalId });
    }
    
    this.SetErrorToast("No se pudo iniciar el formulario");
    return RedirectToAction("Index");
}
```

#### 1.2 New View Model
```csharp
public class ProjectDashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string SelectedProjectName { get; set; } = string.Empty;
    public ProjectDetailsViewModel Project { get; set; } = new();
    public List<AvailableFormViewModel> AvailableForms { get; set; } = new();
    public List<ActivityViewModel> RecentActivities { get; set; } = new();
    
    // Statistics for selected project only
    public int PendingFormsCount => AvailableForms.Count(f => f.Status == FormStatus.Draft);
    public int AvailableFormsCount => AvailableForms.Count(f => !f.IsCreated);
    public int CompletedFormsCount => AvailableForms.Count(f => f.Status == FormStatus.Approved);
}

public class ProjectDetailsViewModel
{
    public long ProjectId { get; set; }
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public DateTime? StageEndDate { get; set; }
    public int Progress { get; set; }
    public string IncubatorName { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class AvailableFormViewModel
{
    public Guid? FormId { get; set; } // Null if not created yet
    public string FormName { get; set; } = string.Empty;
    public QuestionPhase Phase { get; set; }
    public string StageName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsCreated { get; set; }
    public FormStatus Status { get; set; }
    public double CompletionPercentage { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string ActionClass { get; set; } = string.Empty;
}
```

### Phase 2: GetAvailableFormsQuery Implementation

#### 2.1 Query and Handler
```csharp
public record GetAvailableFormsQuery(string UserId, long ProjectId) 
    : IBaseRequest<List<AvailableFormDto>>;

public class AvailableFormDto
{
    public Guid? ExistingFormId { get; set; } // Null if not created
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public QuestionPhase Phase { get; set; }
    public string StageName { get; set; } = string.Empty;
    public ProjectStageType StageType { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCreated { get; set; }
    public ProjectFormSubmissionStatus? Status { get; set; }
    public double CompletionPercentage { get; set; }
    public bool CanStart { get; set; } // Stage is active and within dates
}

public class GetAvailableFormsQueryHandler : BaseCommandHandler<GetAvailableFormsQuery, List<AvailableFormDto>>
{
    public override async Task<Result<List<AvailableFormDto>>> Handle(
        GetAvailableFormsQuery request, 
        CancellationToken cancellationToken)
    {
        var availableForms = new List<AvailableFormDto>();
        
        // Get project with stages
        var project = await repository.GetProjectWithStagesAsync(request.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
            return Success(availableForms);
        
        // Check if user is participant
        var isParticipant = await repository.IsUserProjectParticipantAsync(
            request.ProjectId, request.UserId, cancellationToken);
        if (!isParticipant)
            return Success(availableForms);
        
        // Get project knowledge structure
        var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(
            request.ProjectId, cancellationToken);
        if (knowledgeStructure == null)
            return Success(availableForms); // No forms without structure
        
        // Get existing form submissions for user
        var existingForms = await repository.GetFormSubmissionsByUserAndProjectAsync(
            request.ProjectId, request.UserId, cancellationToken);
        
        var currentDate = timeProvider.UtcNow;
        
        // Check each stage that can have forms
        foreach (var stage in project.ProjectStages.Where(s => s.IsActive))
        {
            // Only InitialFormCollection and FinalFormCollection have forms
            if (stage.Type != ProjectStageType.InitialFormCollection && 
                stage.Type != ProjectStageType.FinalFormCollection)
                continue;
            
            var phase = ProjectFormSubmission.GetPhaseForStage(stage.Type);
            if (phase == QuestionPhase.Undefined)
                continue;
            
            // Check if form exists
            var existingForm = existingForms.FirstOrDefault(f => f.Phase == phase);
            
            var dto = new AvailableFormDto
            {
                ExistingFormId = existingForm?.ExternalId,
                ProjectId = project.Id,
                ProjectName = project.Name,
                Phase = phase,
                StageName = stage.Title,
                StageType = stage.Type,
                DueDate = stage.EndDate,
                IsCreated = existingForm != null,
                Status = existingForm?.Status,
                CompletionPercentage = existingForm?.CompletionPercentage ?? 0,
                CanStart = stage.IsWithinPeriod(currentDate)
            };
            
            availableForms.Add(dto);
        }
        
        return Success(availableForms);
    }
}
```

### Phase 3: Dashboard View Updates

#### 3.1 Updated Index.cshtml
```html
@model ProjectDashboardViewModel
@{
    ViewData["Title"] = $"Panel de Control - {Model.SelectedProjectName}";
    ViewData["PageHeader"] = Model.SelectedProjectName;
    ViewData["BreadcrumbParent"] = "Proyectos";
    ViewData["BreadcrumbActive"] = "Panel de Control";
}

<div class="container-xxl">
    <!-- Project Overview Section -->
    <div class="row">
        <div class="col-12">
            <div class="card mb-4">
                <div class="card-body">
                    <h3 class="mb-3">@Model.Project.Name</h3>
                    <div class="row">
                        <div class="col-md-3">
                            <p class="text-muted mb-1">Estado</p>
                            <h5><span class="badge bg-success">@Model.Project.Status</span></h5>
                        </div>
                        <div class="col-md-3">
                            <p class="text-muted mb-1">Etapa Actual</p>
                            <h5>@Model.Project.CurrentStage</h5>
                        </div>
                        <div class="col-md-3">
                            <p class="text-muted mb-1">Progreso General</p>
                            <div class="progress" style="height: 20px;">
                                <div class="progress-bar" style="width: @Model.Project.Progress%">
                                    @Model.Project.Progress%
                                </div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <p class="text-muted mb-1">Mentor Asignado</p>
                            <h5>@Model.Project.MentorName</h5>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Statistics Cards -->
    <div class="row">
        <div class="col-md-4">
            <div class="card widget-flat">
                <div class="card-body">
                    <h5 class="text-muted fw-normal mt-0">Formularios Disponibles</h5>
                    <h3 class="mt-3 mb-3 text-primary">@Model.AvailableFormsCount</h3>
                    <p class="mb-0 text-muted">
                        <i class="fas fa-play-circle"></i> Listos para iniciar
                    </p>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card widget-flat">
                <div class="card-body">
                    <h5 class="text-muted fw-normal mt-0">En Progreso</h5>
                    <h3 class="mt-3 mb-3 text-warning">@Model.PendingFormsCount</h3>
                    <p class="mb-0 text-muted">
                        <i class="fas fa-edit"></i> Por completar
                    </p>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card widget-flat">
                <div class="card-body">
                    <h5 class="text-muted fw-normal mt-0">Completados</h5>
                    <h3 class="mt-3 mb-3 text-success">@Model.CompletedFormsCount</h3>
                    <p class="mb-0 text-muted">
                        <i class="fas fa-check-circle"></i> Aprobados
                    </p>
                </div>
            </div>
        </div>
    </div>

    <!-- Forms Section -->
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h4 class="header-title mb-0">Formularios del Proyecto</h4>
                </div>
                <div class="card-body">
                    @if (Model.AvailableForms.Any())
                    {
                        <div class="row g-3">
                            @foreach (var form in Model.AvailableForms)
                            {
                                <div class="col-md-6">
                                    <div class="card border">
                                        <div class="card-body">
                                            <div class="d-flex justify-content-between align-items-start mb-3">
                                                <div>
                                                    <h5 class="mb-1">@form.FormName</h5>
                                                    <p class="text-muted mb-2">
                                                        Etapa: @form.StageName
                                                    </p>
                                                    @if (form.DueDate.HasValue)
                                                    {
                                                        var daysRemaining = (form.DueDate.Value.Date - DateTime.UtcNow.Date).Days;
                                                        <p class="mb-0">
                                                            <i class="far fa-clock"></i>
                                                            Fecha límite: @form.DueDate.Value.ToString("dd/MM/yyyy")
                                                            @if (daysRemaining <= 3)
                                                            {
                                                                <span class="badge bg-danger ms-2">@daysRemaining días</span>
                                                            }
                                                        </p>
                                                    }
                                                </div>
                                                <div class="text-end">
                                                    @if (!form.IsCreated)
                                                    {
                                                        <span class="badge bg-info mb-2">Nuevo</span>
                                                    }
                                                    else
                                                    {
                                                        switch (form.Status)
                                                        {
                                                            case ProjectFormSubmissionStatus.Draft:
                                                                <span class="badge bg-warning mb-2">Borrador</span>
                                                                break;
                                                            case ProjectFormSubmissionStatus.Submitted:
                                                                <span class="badge bg-primary mb-2">En Revisión</span>
                                                                break;
                                                            case ProjectFormSubmissionStatus.Approved:
                                                                <span class="badge bg-success mb-2">Aprobado</span>
                                                                break;
                                                            case ProjectFormSubmissionStatus.Rejected:
                                                                <span class="badge bg-danger mb-2">Rechazado</span>
                                                                break;
                                                        }
                                                    }
                                                </div>
                                            </div>
                                            
                                            @if (form.IsCreated && form.Status == ProjectFormSubmissionStatus.Draft)
                                            {
                                                <div class="mb-3">
                                                    <div class="d-flex justify-content-between mb-1">
                                                        <small>Progreso</small>
                                                        <small>@((int)form.CompletionPercentage)%</small>
                                                    </div>
                                                    <div class="progress" style="height: 8px;">
                                                        <div class="progress-bar" style="width: @form.CompletionPercentage%"></div>
                                                    </div>
                                                </div>
                                            }
                                            
                                            <div class="d-grid">
                                                @if (!form.IsCreated)
                                                {
                                                    <a href="@Url.Action("StartForm", new { phase = form.Phase })" 
                                                       class="btn btn-primary">
                                                        <i class="fas fa-play me-2"></i> Iniciar Formulario
                                                    </a>
                                                }
                                                else
                                                {
                                                    switch (form.Status)
                                                    {
                                                        case ProjectFormSubmissionStatus.Draft:
                                                            <a href="/BusinessIncubators/ProjectFormSubmission/Edit/@form.FormId" 
                                                               class="btn btn-warning">
                                                                <i class="fas fa-edit me-2"></i> 
                                                                Continuar (@((int)form.CompletionPercentage)% completado)
                                                            </a>
                                                            break;
                                                        case ProjectFormSubmissionStatus.Submitted:
                                                            <a href="/BusinessIncubators/ProjectFormSubmission/View/@form.FormId" 
                                                               class="btn btn-info">
                                                                <i class="fas fa-eye me-2"></i> Ver Formulario
                                                            </a>
                                                            break;
                                                        case ProjectFormSubmissionStatus.Approved:
                                                            <a href="/BusinessIncubators/ProjectFormSubmission/View/@form.FormId" 
                                                               class="btn btn-success">
                                                                <i class="fas fa-check-circle me-2"></i> Ver Aprobado
                                                            </a>
                                                            break;
                                                        case ProjectFormSubmissionStatus.Rejected:
                                                            <a href="/BusinessIncubators/ProjectFormSubmission/Edit/@form.FormId" 
                                                               class="btn btn-danger">
                                                                <i class="fas fa-redo me-2"></i> Corregir y Reenviar
                                                            </a>
                                                            break;
                                                    }
                                                }
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            }
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-5">
                            <i class="fas fa-clipboard-list fa-3x text-muted mb-3"></i>
                            <h5 class="text-muted">No hay formularios disponibles en este momento</h5>
                            <p class="text-muted">Los formularios estarán disponibles cuando se activen las etapas correspondientes.</p>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>

    <!-- Activity Timeline -->
    <div class="row mt-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h4 class="header-title mb-0">Actividad Reciente del Proyecto</h4>
                </div>
                <div class="card-body">
                    @if (Model.RecentActivities.Any())
                    {
                        <div class="timeline-alt pb-0">
                            @foreach (var activity in Model.RecentActivities)
                            {
                                <div class="timeline-item">
                                    <i class="@activity.Icon bg-@(activity.IconColor)-lighten text-@(activity.IconColor) timeline-icon"></i>
                                    <div class="timeline-item-info">
                                        <strong>@activity.Category</strong>
                                        <p class="mb-1">@activity.Description</p>
                                        <small class="text-muted">@activity.Timestamp.ToString("dd/MM/yyyy HH:mm")</small>
                                    </div>
                                </div>
                            }
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-4">
                            <i class="fas fa-history fa-2x text-muted mb-2"></i>
                            <p class="text-muted">No hay actividad reciente en este proyecto</p>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
</div>
```

### Phase 4: Integration Events and Notifications

#### 4.1 Integration Event
```csharp
public class ProjectStageActivatedIntegrationEvent : IIntegrationEvent
{
    public long ProjectId { get; set; }
    public Guid ProjectExternalId { get; set; }
    public string ProjectName { get; set; }
    public ProjectStageType StageType { get; set; }
    public QuestionPhase Phase { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> ParticipantUserIds { get; set; }
}
```

#### 4.2 Update Stage Commands
In `UpdateProjectStageCommand` or when stages are activated:
```csharp
// After updating stage to active
if (stage.IsActive && !wasActive)
{
    var participants = await repository.GetProjectParticipantsAsync(project.Id, cancellationToken);
    
    var @event = new ProjectStageActivatedIntegrationEvent
    {
        ProjectId = project.Id,
        ProjectExternalId = project.ExternalId,
        ProjectName = project.Name,
        StageType = stage.Type,
        Phase = ProjectFormSubmission.GetPhaseForStage(stage.Type),
        StartDate = stage.StartDate,
        EndDate = stage.EndDate,
        ParticipantUserIds = participants
            .Where(p => p.Role == ParticipantRole.Starter)
            .Select(p => p.UserId)
            .ToList()
    };
    
    await eventBus.PublishAsync(@event, cancellationToken);
}
```

## Implementation Steps

### Phase 1: Project-Scoped Dashboard (2 days)
1. Update `DashboardController` to use `DemandCurrentUserContext`
2. Create `GetProjectDetailsQuery` for single project info
3. Create `ProjectDashboardViewModel` and related models
4. Update dashboard view to show single project
5. Test context enforcement and project display

### Phase 2: Form Discovery (2 days)
1. Create `GetAvailableFormsQuery` and handler
2. Add `StartForm` action to DashboardController
3. Update view to show all available forms
4. Test form discovery and lazy creation flow
5. Verify forms show correct status and actions

### Phase 3: Integration Events (1 day)
1. Create `ProjectStageActivatedIntegrationEvent`
2. Update stage management commands to fire events
3. Create notification handler for email sending
4. Add email templates to database
5. Test notification flow

## Files to Create/Modify

### New Files
- `BusinessIncubator.Application/Queries/GetAvailableFormsQuery.cs`
- `BusinessIncubator.Application/Queries/GetProjectDetailsQuery.cs`
- `BusinessIncubator.Application/Queries/GetProjectActivitiesQuery.cs`
- `BusinessIncubator.Application/IntegrationEvents/ProjectStageActivatedIntegrationEvent.cs`
- `Web/Areas/Participant/Models/ProjectDashboardViewModel.cs`

### Modified Files
- `Web/Areas/Participant/Controllers/DashboardController.cs`
- `Web/Areas/Participant/Views/Dashboard/Index.cshtml`
- `BusinessIncubator.Application/ProjectStages/Commands/UpdateProjectStageCommand.cs`
- `Db/PostDeployment/010.SeedEmailTemplates.sql` (add new templates)

## Testing Requirements

1. **Context Enforcement**:
   - Verify Starters must select project before accessing dashboard
   - Test redirect to context selection if no project selected

2. **Project Scoping**:
   - Confirm dashboard shows only selected project data
   - Verify project switch updates dashboard correctly

3. **Form Discovery**:
   - Test available forms appear based on active stages
   - Verify "Start Form" creates form and redirects correctly
   - Confirm existing forms show proper status and actions

4. **Notifications**:
   - Test email sent when stage activated
   - Verify email contains correct project and form information

## Acceptance Criteria
- [x] Dashboard requires and uses selected project context
- [x] Only selected project information displayed
- [x] Available forms shown based on active stages (not just existing)
- [x] "Start Form" button creates form on-demand
- [x] Existing forms show appropriate status and actions
- [x] Integration event fired when stage activated
- [x] Email notifications sent with direct links
- [x] No duplicate forms created
- [x] Forms respect stage date windows

## Benefits
1. **Focused Experience**: Dashboard shows only relevant project information
2. **Form Discovery**: Users see ALL available forms immediately
3. **Clear Context**: Users always know which project they're working on
4. **Better UX**: Distinct actions based on form status
5. **Maintains Efficiency**: Forms still created lazily on-demand

## Estimated Effort
- Backend implementation: 3 days
- Frontend updates: 2 days
- Testing: 1 day
- **Total: 6 days**