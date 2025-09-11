# LinaSys Web Layer Patterns

## Controller Patterns

### Base Controller Inheritance
```csharp
public class ProjectsController : BaseController  // ✅ Not Controller
{
    // Provides access to helper methods
}
```

### Error Handling Pattern
```csharp
var result = await mediator.Send(command);
if (!result.IsSuccess)
{
    this.MapErrorsToModelStateAndSetErrorToast<ProjectDto>(result);
    return View(model);
}
```

### Success Feedback
```csharp
this.SetSuccessToast("Proyecto creado exitosamente");
return RedirectToAction(nameof(Index));
```

### Toast Notifications
- Requires: `using LinaSys.Web.Extensions;`
- Methods: `SetSuccessToast`, `SetErrorToast`, `SetWarnToast` (not SetWarningToast)

## DataTable Implementation Pattern

### Using DataTable TagHelper
The application uses a custom TagHelper for DataTables. Key points:

1. **TagHelper Handles Initialization**: The DataTable is fully initialized by the TagHelper
2. **Don't Reinitialize**: JavaScript should only get references, not reinitialize
3. **Avoid Duplicate Libraries**: DataTables library is loaded in _BottomPartial

**Correct Pattern**:
```javascript
// Get reference to already initialized DataTable
setTimeout(function() {
    if ($.fn.DataTable.isDataTable('#tableId')) {
        var table = $('#tableId').DataTable();
        // Use table reference for additional operations
    }
}, 100);
```

**Common Mistakes**:
- Loading datatables.js in view's Scripts section (already in _BottomPartial)
- Trying to initialize DataTable again in JavaScript
- Creating custom row actions when TagHelper handles hover actions

## ViewModels and Forms

### Form Validation Pattern
```csharp
public class CreateProjectViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [Display(Name = "Nombre del Proyecto")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string Description { get; set; }
}
```

### File Upload Pattern
```csharp
public class BatchUploadViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un archivo")]
    [Display(Name = "Archivo CSV")]
    public IFormFile CsvFile { get; set; }
}

// Controller converts to Stream
var command = new BatchCommand(
    model.CsvFile.OpenReadStream(),
    model.CsvFile.FileName
);
```

## DataTables Integration

### Server-Side Processing
```csharp
[HttpPost]
public async Task<IActionResult> ListProjects([FromBody] DataTableRequest request)
{
    var query = new ListProjectsQuery(request);
    var result = await mediator.Send(query);
    
    return Json(result.ToDataTableResponse());
}
```

### FilteredQueryResult Pattern
```csharp
public class ListProjectsQueryHandler : IRequestHandler<ListProjectsQuery, Result<FilteredQueryResult<ProjectDto>>>
{
    // Implementation returns paginated, filtered, sorted results
}
```

## Area-Based Organization
```
/Areas/BusinessIncubators/
    /Controllers/
        ProjectsController.cs
    /Models/
        /Project/
            CreateProjectViewModel.cs
            EditProjectViewModel.cs
    /Views/
        /Projects/
            Index.cshtml
            Create.cshtml
/wwwroot/
    /js/
        /businessincubators/
            projectknowledgestructure-edit.js  # ✅ JavaScript here, NOT in Views
            Edit.cshtml
```

## Public vs Protected Controllers

### Protected Area Controllers
```csharp
[Area("BusinessIncubators")]
[Authorize]
public class ProjectsController : BaseController
{
    // Requires authentication
}
```

### Public Controllers (Root)
```csharp
// No area, no authorize attribute
public class InvitationsController : BaseController
{
    // Public access for invitation links
    public async Task<IActionResult> Accept(string token) { }
}
```

## View Patterns

### Layout Selection
```razor
@{
    Layout = "_Layout";  // Authenticated users
    // or
    Layout = "_PublicLayout";  // Public pages
}
```

### Form Structure
```razor
<form asp-action="Create" method="post" class="needs-validation" novalidate>
    <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>
    
    <div class="mb-3">
        <label asp-for="Name" class="form-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">Guardar</button>
</form>
```

### DataTables Configuration
```javascript
$('#projectsTable').DataTable({
    processing: true,
    serverSide: true,
    ajax: {
        url: '@Url.Action("ListProjects")',
        type: 'POST',
        contentType: 'application/json',
        data: function(d) { return JSON.stringify(d); }
    },
    columns: [
        { data: 'name' },
        { data: 'description' },
        { data: 'createdAt', render: formatDate }
    ],
    language: spanishLanguageSettings
});
```

## Result Handling Patterns

### TempData for Complex Objects
```csharp
// Controller
TempData["BatchResults"] = JsonSerializer.Serialize(results);
return RedirectToAction(nameof(BatchResults));

// View
@{
    var results = TempData["BatchResults"] != null 
        ? JsonSerializer.Deserialize<BatchResultsViewModel>(TempData["BatchResults"].ToString())
        : null;
}
```

### Error Context Mapping
```csharp
// In ResultContextToViewModelMapper
case "CreateProject":
    return "Error al crear el proyecto";
```

## Security Patterns

### CSRF Protection
- Automatically included in forms with tag helpers
- Manual: `@Html.AntiForgeryToken()`

### Input Validation
- Server-side validation always required
- Client-side for UX only
- Sanitize all inputs
- File upload validation (size, type, content)

## Bootstrap Integration
- Use Bootstrap 5 classes
- Consistent spacing: `mb-3`, `mt-4`, etc.
- Responsive tables: `table-responsive`
- Form controls: `form-control`, `form-select`
- Buttons: `btn btn-primary`, `btn btn-secondary`

## Phoenix Admin Template Compliance

### Use Only Template Components
**Policy**: Use only Phoenix Admin Template built-in components. Never add external libraries.

### Modal Dialogs
Use Bootstrap modals instead of external libraries:
```javascript
// Create Bootstrap modal helper
function showConfirmModal(title, message, confirmText, cancelText) {
    return new Promise((resolve) => {
        const modalHtml = `
            <div class="modal fade" id="confirmModal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p>${message}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
                            <button type="button" class="btn btn-primary" id="confirmBtn">${confirmText}</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        $('#confirmModal').remove();
        $('body').append(modalHtml);
        
        const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
        
        $('#confirmBtn').on('click', () => {
            modal.hide();
            resolve(true);
        });
        
        document.getElementById('confirmModal').addEventListener('hidden.bs.modal', () => {
            $('#confirmModal').remove();
            resolve(false);
        });
        
        modal.show();
    });
}
```

### Toast Notifications
Use Bootstrap toasts for notifications:
```javascript
function showToast(message, type = 'info') {
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type} border-0">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    
    const toastElement = $(toastHtml);
    $('#toastContainer').append(toastElement);
    const toast = new bootstrap.Toast(toastElement[0]);
    toast.show();
}
```

### Avoid External Dependencies
- ❌ NO SweetAlert2
- ❌ NO custom notification libraries
- ❌ NO external wizard libraries
- ✅ Use Bootstrap components
- ✅ Use Phoenix theme utilities

## Tree View Pattern (jsTree)

### Basic Setup
```javascript
$('#treeContainer').jstree({
    'core': {
        'data': {
            'url': '/api/tree-data',
            'dataType': 'json'
        },
        'check_callback': true  // Allow modifications
    },
    'types': {
        'root': { 'icon': 'ti ti-sitemap' },
        'folder': { 'icon': 'ti ti-folder' },
        'file': { 'icon': 'ti ti-file' }
    },
    'plugins': ['types', 'contextmenu', 'dnd', 'search']
});
```

### Node ID Handling
```javascript
// Always use helper for consistent ID extraction
function getNodeEntityId(node) {
    if (!node || !node.data) return null;
    return node.data.entityId || node.data.id || null;
}
```

### Global Variables for External JS
```javascript
// Declare at window level in view
var businessIncubatorId = '@Model.BusinessIncubatorId';
var projectId = '@Model.ProjectId';

// Use in external JS files
url: `/BusinessIncubators/${businessIncubatorId}/Projects/${projectId}/action`
```

## Wizard Implementation Pattern

### Phoenix Admin Template Wizard
The application uses Phoenix Admin Template's wizard component for multi-step forms.

#### HTML Structure
```razor
<div class="theme-wizard" data-theme-wizard="data-theme-wizard">
    <!-- Wizard Navigation -->
    <ul class="nav nav-wizard nav-wizard-success">
        @for (int i = 0; i < Model.Steps.Count; i++)
        {
            <li class="nav-item @(step.IsCompleted ? "done" : "")">
                <a class="nav-link @(i == 0 ? "active" : "")"
                   href="#step-@i"
                   data-bs-toggle="tab"
                   data-wizard-step="@(i + 1)">
                    <span class="nav-item-circle-parent">
                        <span class="nav-item-circle">
                            <span class="fas fa-check check-icon"></span>
                        </span>
                    </span>
                    <span class="nav-item-title">@step.Title</span>
                </a>
            </li>
        }
    </ul>
    
    <!-- Tab Content -->
    <div class="tab-content">
        <!-- Step content here -->
    </div>
    
    <!-- Navigation Footer -->
    <div class="card-footer" data-wizard-footer="data-wizard-footer">
        <button id="btnPrevious" data-wizard-prev-btn>Previous</button>
        <button id="btnNext" data-wizard-next-btn>Next</button>
    </div>
</div>
```

#### JavaScript Pattern

### Form Wizard Implementation
```javascript
// Main wizard controller pattern
class EntrepreneurFormWizard {
    constructor() {
        this.projectId = $('#projectExternalId').val();
        this.currentStep = 1;
        this.isDirty = false;
        this.autoSaveManager = new FormAutoSaveManager(this);
        this.validator = new FormValidator();
        this.init();
    }
    
    // Phoenix wizard integration
    initializeWizard() {
        $('.nav-wizard .nav-link').on('shown.bs.tab', (e) => {
            this.onStepChange(e);
        });
    }
    
    // Auto-save on step change
    onStepChange(event) {
        if (this.isDirty) {
            this.saveDraft(false);
        }
    }
}

// Auto-save manager with debouncing
class FormAutoSaveManager {
    scheduleAutoSave() {
        clearTimeout(this.saveTimeout);
        this.saveTimeout = setTimeout(() => {
            this.wizard.saveDraft(false);
        }, 5000); // 5-second debounce
    }
    
    showSaving() {
        $('#autoSaveIndicator').removeClass('d-none');
        $('#saveSpinner').removeClass('d-none');
    }
}

// Toast notifications
function showToast(message, type = 'info') {
    const toastHtml = `<div class="toast bg-${type}">...</div>`;
    $('#toastContainer').append($(toastHtml));
    new bootstrap.Toast(toastElement[0]).show();
}
```

**Auto-save Strategy**:
- 30-second interval for automatic saves
- 5-second debounce after last change
- Visual feedback with spinner/checkmark
- Error handling with retry logic

#### JavaScript Pattern
```javascript
class FormWizard {
    constructor() {
        this.currentStep = 1;
        this.totalSteps = $('.nav-wizard .nav-item').length;
        this.isDirty = false;
        
        // Initialize managers
        this.autoSaveManager = new FormAutoSaveManager(this);
        this.validator = new FormValidator();
        
        this.init();
    }
    
    init() {
        // Phoenix wizard uses Bootstrap tabs
        $('.nav-wizard .nav-link').on('shown.bs.tab', (e) => {
            this.onStepChange(e);
        });
        
        // Navigation buttons
        $('#btnPrevious').on('click', () => this.navigateToPrevious());
        $('#btnNext').on('click', () => this.navigateToNext());
    }
    
    onStepChange(event) {
        const newStep = parseInt($(event.target).data('wizard-step'));
        
        // Validate before moving forward
        if (newStep > this.currentStep) {
            if (!this.validateStep(this.currentStep)) {
                event.preventDefault();
                return false;
            }
        }
        
        this.currentStep = newStep;
        this.updateNavigationButtons();
        
        // Auto-save on step change
        if (this.isDirty) {
            this.saveDraft();
        }
    }
    
    validateStep(stepNumber) {
        const form = document.querySelector(`[data-wizard-form="${stepNumber}"]`);
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
            return false;
        }
        return true;
    }
}
```

### Auto-Save Pattern
```javascript
class FormAutoSaveManager {
    constructor(wizard) {
        this.wizard = wizard;
        this.saveTimeout = null;
        this.autoSaveInterval = 30000; // 30 seconds
        
        // Start auto-save timer
        setInterval(() => {
            if (this.wizard.isDirty) {
                this.wizard.saveDraft(false);
            }
        }, this.autoSaveInterval);
    }
    
    showSaving() {
        $('#autoSaveIndicator').removeClass('d-none');
        $('#saveSpinner').removeClass('d-none');
        $('#saveCheck').addClass('d-none');
        $('#saveText').text('Guardando...');
    }
    
    showSaved(timestamp) {
        $('#saveSpinner').addClass('d-none');
        $('#saveCheck').removeClass('d-none');
        $('#saveText').text('Guardado');
        $('#saveTime').text(timestamp);
        
        setTimeout(() => {
            $('#autoSaveIndicator').addClass('d-none');
        }, 3000);
    }
}
```

### Progress Tracking
```javascript
updateProgress() {
    const allInputs = $('.question-input');
    const answeredInputs = allInputs.filter(function() {
        const input = $(this);
        if (input.is(':checkbox') || input.is(':radio')) {
            return input.is(':checked');
        }
        return input.val() && input.val().trim() !== '';
    });
    
    const percentage = Math.round((answeredInputs.length / allInputs.length) * 100);
    
    $('#progressBar').css('width', percentage + '%');
    $('#progressPercent').text(percentage);
    
    return percentage;
}
```

### Key Wizard Features
- **Step Validation**: Each step validated before progression
- **Auto-Save**: Every 30 seconds and on step change
- **Progress Bar**: Visual progress indicator
- **Draft Restoration**: Restore saved data on page reload
- **Mobile Responsive**: Adapts navigation for mobile
- **Keyboard Support**: Ctrl+S to save draft

## Dual Answer Review Pattern

### Implementation for Coordinator Review (REQ-008)
When coordinators need to provide their own answers alongside participant responses:

#### JavaScript Manager Pattern
```javascript
window.DualAnswerReviewManager = (function() {
    let coordinatorAnswers = {};
    let autoSaveInterval = 30000; // 30 seconds
    
    function copyFromStarter(questionId) {
        coordinatorAnswers[questionId] = starterAnswers[questionId];
        updateProgress();
        checkForDifferences(questionId);
    }
    
    async function saveDraft(showNotification = true) {
        // Auto-save implementation
    }
    
    return {
        init: init,
        renderDualAnswerLayout: renderDualAnswerLayout,
        saveDraft: saveDraft,
        validateCompletion: validateCompletion
    };
})();
```

#### CSS Grid Layout
```css
.dual-answer-container {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
}

@media (max-width: 768px) {
    .dual-answer-container {
        grid-template-columns: 1fr;
    }
}
```

#### Integration with Approval
```javascript
// In handleApprove function
if (window.DualAnswerReviewManager && !window.DualAnswerReviewManager.validateCompletion()) {
    showToast('Debe completar todas sus respuestas como coordinador antes de aprobar', 'warning');
    return;
}
```

## ViewComponent Patterns

### Creating a ViewComponent
```csharp
public class UserContextViewComponent(
    MediatorExecutor mediatorExecutor,
    ILogger<UserContextViewComponent> logger) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Fetch data using MediatorExecutor
        var result = await mediatorExecutor.SendAndLogIfFailureAsync(query);
        
        // Return view with model
        return View(model);
    }
}
```

### ViewComponent View Location
```
/Views/Shared/Components/{ComponentName}/Default.cshtml
```

### Invoking ViewComponent in Razor
```razor
@await Component.InvokeAsync("UserContext")
```

### ViewComponent Model Pattern
```csharp
public class UserContextDisplayViewModel
{
    // Data properties
    public string? Role { get; set; }
    public string? IncubatorName { get; set; }
    
    // Control flags for view logic
    public bool HasContext { get; set; }
    public bool ShowIncubator { get; set; }
}
```
- Separate display control flags from data
- Allows clean conditional rendering in view

### Tree Data Structure
```csharp
public class TreeNodeDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } // Unique identifier like "block_123"
    
    [JsonPropertyName("text")]
    public string Text { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; } // For icon/styling
    
    [JsonPropertyName("data")]
    public TreeNodeDataDto Data { get; set; } // Custom data
    
    [JsonPropertyName("children")]
    public List<TreeNodeDto> Children { get; set; } = new();
}
```