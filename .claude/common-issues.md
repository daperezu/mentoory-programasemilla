# LinaSys Common Issues & Solutions

## Dependency Injection & Service Issues

### ITimeProvider Not Found
**Error**: `CS0246: The type or namespace name 'ITimeProvider' could not be found`
**Solution**: Use correct namespace:
```csharp
// ❌ Wrong
using LinaSys.Shared.Domain.Services;

// ✅ Correct
using LinaSys.Shared.Application.TimeProvider;
```
**Pattern**: ITimeProvider is in Application layer, not Domain

### ResultErrorCodes Missing Definition
**Error**: `CS0117: 'ResultErrorCodes' does not contain a definition for 'BusinessValidationError'`
**Solution**: Use existing error codes:
```csharp
// For domain validation errors
ResultErrorCodes.GenericError

// For not found errors
ResultErrorCodes.BusinessIncubator_NotFound
```

## Performance Issues

### Dashboard Loading Takes 5-10 Seconds
**Problem**: Dashboard executes 20+ queries causing slow load times.

**Root Causes**:
- N+1 queries when loading user names
- Multiple handlers loading same project data
- No database-level aggregation
- Missing indexes on critical columns

**Solution**: Create single optimized query:
```csharp
// Handler with single query + caching
public class GetCoordinatorDashboardCompleteDataQueryHandler(
    IBusinessIncubatorRepository repository,
    IMemoryCache cache,
    ITimeProvider timeProvider)
{
    // Get all data in ONE query with DB aggregation
    var dashboardData = await repository.GetProjectDashboardDataAsync(
        projectId, timeProvider.UtcNow);
    
    // Batch load users to avoid N+1
    var users = await mediator.Send(new GetUsersByIdsQuery(userIds));
    
    // Cache for 5 minutes
    cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
}
```

**Key Patterns**:
- Use `GetUsersByIdsAsync` for batch user loading
- Pass `ITimeProvider.UtcNow` to repository methods
- Use `HttpContext.Items` for request-scoped data (not custom cache)
- No SQL scripts needed pre-production
- Missing database indexes on critical columns
- All filtering done in-memory instead of at database level

**Solution**: 
```csharp
// ❌ WRONG - N+1 queries
foreach (var submission in submissions) {
    var user = await userManager.FindByIdAsync(submission.UserId);
}

// ✅ CORRECT - Batch load users
var userIds = submissions.Select(s => s.UserId).Distinct();
var users = await authRepository.GetUsersByIdsAsync(userIds);
```

**Quick Wins**:
1. Add database indexes on frequently queried columns
2. Use `.AsNoTracking()` for read-only queries
3. Project to DTOs using `.Select()` instead of loading full entities
4. Cache data within request scope to avoid duplicate loads

## Entity Framework Issues

### Include with String-Based Backing Field
**Error**: `InvalidIncludePathError: Unable to find navigation '_answers'`
**Cause**: Using backing field name in Include instead of navigation property
**Solution**: Use lambda expression with public property
```csharp
// ❌ WRONG - Using backing field name
.Include("_answers")

// ✅ CORRECT - Use public navigation property
.Include(d => d.Answers)
```

### Repository Method Not Loading Required Data
**Error**: Questions display as plain text, answers show as "Sin respuesta"
**Cause**: Repository method not including all required related entities for query
**Solution**: Ensure repository methods include all necessary navigation properties

```csharp
// ❌ WRONG - Missing ProjectBlocks needed by handler
public async Task<Project?> GetProjectWithKnowledgeStructureByIdAsync(long projectId)
{
    return await dbContext.Set<Project>()
        .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectSubjects.ProjectSubjectResources")
        .FirstOrDefaultAsync(p => p.Id == projectId);
}

// ✅ CORRECT - Include all entities needed by the query handler
public async Task<Project?> GetProjectWithKnowledgeStructureByIdAsync(long projectId)
{
    return await dbContext.Set<Project>()
        .Include("ProjectKnowledgeStructure.ProjectModules.ProjectTopics.ProjectSubjects.ProjectSubjectResources")
        .Include("ProjectBlocks.ProjectQuestions.ProjectAnswerOptions")  // Include blocks for questionMap
        .FirstOrDefaultAsync(p => p.Id == projectId);
}
```

**Diagnosis Steps**:
1. Check what entities the handler is trying to access (e.g., `project.ProjectBlocks`)
2. Verify those entities are included in the repository method
3. If handler builds maps or lookups, ensure source data is loaded
4. Watch for empty collections that should have data

### DbContext Missing Entity Error
**Error**: `Cannot create a DbSet for 'EntityName' because this type is not included in the model`
**Solution**: 
- Add DbSet property to DbContext: `public virtual DbSet<EntityName> EntityNames { get; set; }`
- Add entity configuration in OnModelCreating
- Configure navigation properties with backing fields if needed:
  ```csharp
  entity.Navigation(e => e.CollectionProperty)
      .UsePropertyAccessMode(PropertyAccessMode.Field)
      .HasField("_backingField");
  ```

### Entity Framework Tracked Entity Updates
**Error**: `CS1503: cannot convert from 'Entity' to 'BusinessIncubator'` when calling repository.Update()
**Cause**: Trying to update an already-tracked entity
**Solution**: Remove the Update call - tracked entities are automatically updated
```csharp
// ❌ WRONG - Entity already tracked after retrieval
var entity = await repository.GetByIdAsync(id);
entity.ModifyProperty(value);
repository.Update(entity); // Unnecessary!

// ✅ CORRECT - Just save changes
var entity = await repository.GetByIdAsync(id);
entity.ModifyProperty(value);
await repository.UnitOfWork.SaveChangesAsync();
```

### EF Core Duplicate Foreign Key Columns
**Error**: `Invalid column name 'PropertyId1'`
**Cause**: Navigation property not properly configured
**Solution**: Use navigation property in HasOne instead of generic type:
```csharp
// Wrong: entity.HasOne<RelatedEntity>()
// Right: entity.HasOne(e => e.NavigationProperty)
```

### EF Core Include with DDD Private Collections
**Error**: `InvalidIncludePathError: Unable to find navigation '_fieldName'`
**Cause**: Different configuration patterns for private backing fields
**Solution**: Check DbContext configuration to determine correct Include approach:

```csharp
// Case 1: Property explicitly ignored - use private field
modelBuilder.Entity<Project>().Ignore(p => p.ProjectStages);
// Include: .Include("_projectStages")

// Case 2: Navigation configured with backing field - use public property
entity.Navigation(e => e.ProjectUsers)
    .UsePropertyAccessMode(PropertyAccessMode.Field)
    .HasField("_projectUsers");
// Include: .Include("ProjectUsers")
```

## Modern UI Implementation Patterns (Phoenix Admin Template)

### Gradient Backgrounds
**Pattern**: Use CSS gradients for headers and active states
```css
/* Primary gradient pattern */
background: linear-gradient(135deg, var(--phoenix-primary), #2c5cc5);
```

### Glassmorphism Effects
**Pattern**: Combine backdrop-filter with semi-transparent backgrounds
```css
.phoenix-card {
    background: rgba(var(--phoenix-body-bg-rgb), 0.98);
    backdrop-filter: blur(10px);
}
```

### Animation Performance
**Important**: Always use transform and opacity for animations (GPU-accelerated)
```css
/* ✅ Good - GPU accelerated */
@keyframes slideIn {
    from { transform: translateX(-100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
}

/* ❌ Avoid - CPU intensive */
@keyframes slideIn {
    from { left: -100%; }
    to { left: 0; }
}
```

### Real-time UI Updates Pattern
**Pattern**: Update UI immediately on field change, not just on save
```javascript
// Attach to all form inputs
element.addEventListener('change', () => {
    this.onFieldChange(element);  // Pass element for context
    this.updateProgress();         // Update immediately
    this.animateCompletion();      // Visual feedback
    this.scheduleSave();           // Debounced save
});
```

### Icon Mapping for Form Fields
**Pattern**: Use consistent icon mapping for question types
```javascript
const iconMap = {
    1: 'fa-dot-circle',      // SingleChoice
    2: 'fa-check-square',    // MultiChoice
    3: 'fa-align-left',      // FreeText
    4: 'fa-sort-numeric-up', // Numeric
    5: 'fa-calendar-alt',    // Date
    6: 'fa-link'             // Url
};
```

## NuGet Package Resolution in Central Package Management

### Missing Types in Infrastructure Projects
**Error**: `CS0246: The type or namespace name 'HttpClient' could not be found`
**Cause**: Missing package reference in Directory.Packages.props when using central package management

**Solution**: Add missing package to Directory.Packages.props (NOT to individual project files)
```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Microsoft.Extensions.Http" Version="9.0.0" />
```

**Important**: When infrastructure code appears correct but types are missing:
1. Don't modify working code
2. Check Directory.Packages.props for missing package references  
3. Add the appropriate Microsoft.Extensions.* package
4. Project files should only reference packages by name (no version)

## .NET Aspire Azure Storage Integration

### RunAsEmulator() Not Automatic
**Error**: Azurite container doesn't start automatically
**Cause**: Must explicitly call `RunAsEmulator()` for local development

**Solution**:
```csharp
// ❌ Wrong - Azurite won't start
var storage = builder.AddAzureStorage("storage");
var blobs = storage.AddBlobs("blobs");

// ✅ Correct - Explicit emulator configuration
if (builder.ExecutionContext.IsRunMode)
{
    var storage = builder.AddAzureStorage("storage").RunAsEmulator();
    blobs = storage.AddBlobs("blobs");
}
else
{
    var storage = builder.AddAzureStorage("storage");
    blobs = storage.AddBlobs("blobs");
}
```

### Connection String Naming
**Issue**: BlobServiceClient not injected
**Cause**: Connection string name must match AddBlobs() parameter

**Solution**:
```csharp
// AppHost: AddBlobs("blobs")
// Infrastructure: Connection string will be "blobs" not "AvatarStorage"
builder.AddAzureBlobClient("blobs"); // Must match name
```

## Clean Architecture Violations

### Circular Dependency Between Layers
**Error**: `MSB4006: There is a circular dependency in the target dependency graph`
**Cause**: Application layer referencing Infrastructure layer (violates Clean Architecture)

**Solution**: 
- Application layer should only reference Domain layer
- Use repository interfaces with UnitOfWork pattern
- Move cross-domain services to Web layer if they need multiple project references

```csharp
// ❌ Wrong - Application directly using DbContext
public class Handler(UserManagementDbContext dbContext) { 
    await dbContext.SaveChangesAsync(); 
}

// ✅ Correct - Using repository with UnitOfWork
public class Handler(IUserProfileRepository repository) { 
    repository.Update(entity);
    await repository.UnitOfWork.SaveChangesAsync(); 
}
```

**Cross-Domain Service Location**:
```csharp
// ❌ Wrong - In Infrastructure layer causes circular deps
LinaSys.UserManagement.Infrastructure/Services/UserCrossDomainService.cs

// ✅ Correct - In Web layer which can reference all projects
LinaSys.Web/Services/UserCrossDomainService.cs
```

## Build Error Resolution Process
1. **Identify error categories**: Compilation vs StyleCop vs View compilation
2. **Fix in dependency order**: Domain → Application → Infrastructure → Web → Views
3. **API mismatches**: Check property names and result object structures
4. **Extension methods**: Verify using statements and controller inheritance
5. **View bindings**: Match DTO properties exactly with Razor view references
6. **Null safety**: Use proper null patterns (`is null`, `?.`, `??`)
7. **File formatting**: Remove trailing whitespace, add newlines, fix commas
8. **Enum ambiguity**: Use fully qualified names when duplicate enum names exist
8. **Build verification**: Clean build required before testing

## DataTable Custom Rendering

### Custom Render Functions Not Working
**Error**: Custom render function not being called
**Cause**: Function not accessible globally or incorrect setup

**Solution**:
```javascript
// ✅ Correct - Define globally
window.renderRoleBadges = function(data, type, row) {
    if (type !== 'display') return data;
    // Rendering logic here
};
```

```csharp
// In Razor view
new DatatableColumn {
    Data = "roles",
    RenderType = ColRenderType.Custom,
    RenderJs = "renderRoleBadges"  // Without 'window.' prefix
}
```

### JavaScript String Literals in TagHelpers
**Error**: CS1002: ; expected (compilation errors in TagHelper)
**Cause**: Double quotes in JavaScript strings conflict with C# string interpolation

**Solution**:
```csharp
// ❌ Wrong - Double quotes cause C# compilation errors
language: {{ processing: "Procesando..." }}

// ✅ Correct - Use single quotes for JavaScript strings
language: {{ processing: 'Procesando...' }}
```

### Context-Aware DataTable Actions
**Pattern**: Dynamic actions based on row data

```javascript
$('#tableId').on('draw.dt', function() {
    $('.hover-actions-trigger').each(function() {
        const data = table.row(this).data();
        // Replace placeholder with context-aware action
        if (!data.isActive) {
            // Show reactivate button
        } else if (data.invitationStatus === 'pending') {
            // Show resend invitation button
        }
    });
});
```

## CSV Export with Spanish Characters

### Excel Not Reading UTF-8 Properly
**Issue**: Spanish characters appear corrupted in Excel
**Cause**: Missing UTF-8 BOM

**Solution**:
```javascript
// Add UTF-8 BOM for Excel compatibility
const blob = new Blob(['\ufeff' + csvContent], { 
    type: 'text/csv;charset=utf-8;' 
});
```

## StyleCop Analyzer Errors

### SA1028: Trailing Whitespace
**Error**: `error SA1028: Code should not contain trailing whitespace`
**Common Locations**:
- After closing braces
- Empty lines with spaces  
- After variable declarations
- Between method parameters

**Solution**: Remove all trailing spaces
```csharp
// Wrong
var userRoles = CurrentUserRoles;
        
// Correct  
var userRoles = CurrentUserRoles;

// Use MultiEdit for bulk fixes across file
```

**Prevention**: Configure IDE to trim trailing whitespace on save

### SA1407: Arithmetic Expressions Should Declare Precedence
**Error**: `error SA1407: Arithmetic expressions should declare precedence`
**Cause**: Complex mathematical expressions without explicit parentheses
**Solution**: Add parentheses to clarify operator precedence
```csharp
// ❌ Wrong
double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
           Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
           Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

// ✅ Correct  
double a = (Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)) +
           (Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2));
```

### Failure Method Parameter Format
**Error**: `CS1503: cannot convert from 'string' to '(string Context, string Message)'`
**Cause**: BaseCommandHandler Failure method expects tuple format
**Solution**: 
```csharp
// ❌ Wrong
return Failure(ResultErrorCodes.GenericError, "Error message");

// ✅ Correct
return Failure(ResultErrorCodes.GenericError, 
    (nameof(GetNearbyProjectsQuery), "Error message"));
```

### EF Core Navigation Property Access in Lambda
**Error**: `CS0122: 'Project.BusinessIncubator' is inaccessible due to its protection level`
**Cause**: Internal navigation properties cannot be accessed in Lambda expressions
**Solution**: Use string-based Include instead:
```csharp
// ❌ Wrong
.Include(p => p.BusinessIncubator)

// ✅ Correct
.Include("BusinessIncubator")
```

## SQL Server Database Project Issues

### Schema Changes in Wrong Location
**Error**: Database changes not applying or build errors
**Cause**: Creating post-deployment scripts for schema changes

**Wrong Approach**:
```sql
-- ❌ DON'T create PostDeployment\011.AddColumns.sql
ALTER TABLE [businessincubators].[ProjectFormSubmissions]
ADD [Phase] INT NOT NULL DEFAULT 1;
```

**Correct Approach**:
```sql
-- ✅ Update the table definition file directly
-- LindaDb\businessincubators\Tables\ProjectFormSubmissions.sql
CREATE TABLE [businessincubators].[ProjectFormSubmissions] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Phase] INT NOT NULL DEFAULT 1,  -- Add here
    -- other columns...
);
```

**Index Creation**:
- Create separate files in `LindaDb\[schema]\Indexes\` folder
- One index per file with descriptive names

## Identity Configuration Issues

### Username Validation Mismatch
**Error**: User registration fails with "Invalid username" 
**Cause**: Validation doesn't match Microsoft Identity configuration

**Wrong**:
```csharp
// Validation allows alphanumeric
.Matches(@"^[a-zA-Z0-9\-_.]+$")

// But Identity only accepts numeric
opts.User.AllowedUserNameCharacters = "0123456789";
```

**Correct**:
```csharp
// Validation matches Identity configuration
RuleFor(x => x.Identification)
    .Matches(@"^[0-9]+$")
    .WithMessage("La identificación debe contener solo números.");
```

## Domain Model Confusion

### Multiple Entities with Same Name
**Issue**: Two UserProfile entities in different domains causing confusion
**Example**: Auth.Domain.UserProfile vs UserManagement.Domain.UserProfile

**Solution**:
1. Identify which domain truly owns the entity
2. Remove redundant entities from other domains
3. Use orchestration for cross-domain operations

**Pattern**:
```csharp
// Auth domain - authentication only
public record CreateUserCommand(Email, Password) : IBaseRequest<User>;

// UserManagement domain - profile management
public record CreateUserProfileCommand(UserId, FirstName, LastName) : IBaseRequest<long>;

// Orchestration - coordinates both
public class CreateUserWithProfileOrchestrationCommand {
    // Calls Auth.CreateUserCommand
    // Then calls UserManagement.CreateUserProfileCommand
}
```

## Entity Null Comparison Issues

### CS8604/CS8625 with Entity Types
**Error**: `Possible null reference argument for parameter 'left' in 'bool Entity.operator ==(Entity left, Entity right)'`

**Wrong Pattern**:
```csharp
if (project == null)  // CS8604 error
if (stage != null)    // CS8604 error
```

**Correct Pattern**:
```csharp
if (project is null)      // ✅ Use pattern matching
if (stage is not null)    // ✅ Use pattern matching
```

**Reason**: Entity base class has custom equality operators that don't handle nulls properly with nullable reference types enabled

## Controller Implementation Errors

### CS0266: QuestionPhase Enum Ambiguity
**Error**: Cannot implicitly convert type 'LinaSys.BusinessIncubator.Domain.Enums.QuestionPhase' to 'LinaSys.Web.Models.QuestionPhase'
**Solution**: Use fully qualified names
```csharp
public LinaSys.BusinessIncubator.Domain.Enums.QuestionPhase Phase { get; set; }
```

### CS7036: BaseController Constructor Missing Parameters
**Error**: No argument given for required parameter 'logger' of 'BaseController.BaseController(ILogger, MediatorExecutor)'
**Solution**: Use primary constructor pattern
```csharp
public class EntrepreneurFormController(
    ILogger<EntrepreneurFormController> logger,
    MediatorExecutor mediatorExecutor,
    IMediator mediator) : BaseController(logger, mediatorExecutor)
```

### GetUserId Extension Method Not Found
**Error**: 'ClaimsPrincipal' does not contain a definition for 'GetUserId'
**Solution**: Use UserManager instead
```csharp
private readonly UserManager<User> _userManager;
var userId = _userManager.GetUserId(User);
```

## Entity Framework Configuration Issues

### Backing Field Configuration Conflict
**Error**: `The member 'Entity._field' cannot use field '_field' because it is already used by 'Entity.Property'`

**Cause**: Trying to use backing field directly in relationship configuration while also configuring it as navigation property

**Solution**:
```csharp
// Wrong
entity.HasOne<Parent>()
    .WithMany("_childrenField")  // Don't use backing field directly
    
// Correct
entity.HasOne<Parent>()
    .WithMany(e => e.Children)   // Use the public property
```

### Duplicate Entity Table Mapping
**Error**: `Cannot use table 'schema.TableName' for entity type 'EntityA' since it is being used for entity type 'EntityB'`

**Cause**: Two different entity classes configured to use the same database table

**Solution**:
1. Remove duplicate DbSet declarations
2. Remove duplicate entity configurations
3. Keep only one entity mapped to each table

```csharp
// Remove old entity DbSet
// public DbSet<OldEntity> OldEntities { get; set; }

// Remove old entity configuration
// modelBuilder.Entity<OldEntity>(...);
```

## SQL Database Project Build Issues

### MSBuild Parameter Syntax
```bash
# Problem: MSBuild fails with parameter parsing error
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" LinaDb.sqlproj /p:Configuration=Debug
# Error: MSB1008: Only one project can be specified

# Solution: Use hyphen instead of forward slash for parameters
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" LinaDb.sqlproj -p:Configuration=Debug -v:minimal
# ✅ Builds successfully
```

### PostDeployment Script Organization
```sql
-- Problem: New seed script not executing
-- Solution: Add to Script.PostDeployment.sql in correct order

PRINT '[007.SeedAuthAccessTables.sql] Starting';
:r .\007.SeedAuthAccessTables.sql
PRINT '[007.SeedAuthAccessTables.sql] Finished';
```

### PostDeployment Script Configuration
```xml
<!-- Problem: SQL71006 - Only one statement is allowed per batch -->
<!-- Wrong: Post-deployment scripts as Build items -->
<Build Include="PostDeployment\008.SeedNavigationMenuItems.sql" />

<!-- Correct: Post-deployment scripts as None items -->
<None Include="PostDeployment\008.SeedNavigationMenuItems.sql" />
```

### PostDeployment Script Batch Restrictions
```sql
-- Problem: DBCC commands fail in post-deployment scripts
DELETE FROM [table];
DBCC CHECKIDENT ('[table]', RESEED, 0);  -- ❌ Fails

-- Solution: Remove DBCC, use simple DELETE
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TableName')
BEGIN
    DELETE FROM [schema].[TableName];  -- ✅ Works
END
```

## StyleCop Analyzer Issues

### SA1208: Using Directive Order
```csharp
// Problem: Using directives not in correct order
// Error: SA1208: Using directive for 'System.Collections.Generic' should appear before directive for 'LinaSys.Shared.Domain.SeedWork'

// Wrong order:
using LinaSys.Shared.Domain.SeedWork;
using System.Collections.Generic;

// Correct order (System namespaces first):
using System.Collections.Generic;
using LinaSys.Shared.Domain.SeedWork;
```

### SA1202: Member Ordering
```csharp
// Problem: Protected members before public members
// Error: SA1202: 'public' members should come before 'protected' members

// Wrong order:
protected NavigationMenuItem() { }
public NavigationMenuItem(string code) { }

// Correct order:
public NavigationMenuItem(string code) { }
protected NavigationMenuItem() { }
```

## Common Compilation Errors

### Circular Dependencies Between Application Projects
```csharp
// Problem: Auth.Application references BusinessIncubator.Application and vice versa
// Error: MSB4006: Circular dependency in target dependency graph

// Solution: Move shared contracts to LinaSys.Shared.Application
// Before:
using LinaSys.BusinessIncubator.Application.IntegrationEvents;

// After:
using LinaSys.Shared.Application.IntegrationEvents.Auth;
```

### Entity Name Changes After Refactoring
```csharp
// Problem: Old entity names after refactoring
_context.ProjectUsers  // ❌ CS1061: Does not contain definition
_context.IncubatorUsers  // ❌ CS1061: Does not contain definition

// Solution: Use new entity names
_context.UserProjectAccesses  // ✅ New access entity
_context.UserIncubatorAccesses  // ✅ New access entity
_context.UserMentorshipAccesses  // ✅ New access entity
```

### Namespace Conflicts
```csharp
// Problem: 'Project' conflicts with namespace
Project project = new Project();  // ❌ Ambiguous

// Solution: Use fully qualified name
BusinessIncubator.Domain.Aggregates.BusinessIncubator.Project project;  // ✅
```

### Missing Using Statements
```csharp
// Problem: SetSuccessToast not found
this.SetSuccessToast("Éxito");  // ❌ Method not found

// Solution: Add using
using LinaSys.Web.Extensions;  // ✅
```

### Result API Mismatches
```csharp
// Problem: Wrong property name
var data = result.Data;  // ❌ 'Data' doesn't exist

// Solution: Check actual property
var data = result.Value;  // ✅ Correct property name

// Problem: Wrong success check property
if (!contextResult.Succeeded)  // ❌ 'Succeeded' doesn't exist

// Solution: Use IsSuccess property
if (!contextResult.IsSuccess)  // ✅ Correct property name
```

## View Compilation Errors

### DTO Property Mismatches
```razor
@* Problem: Property doesn't exist *@
<td>@item.ProjectName</td>  @* ❌ No such property *@

@* Solution: Check actual DTO *@
<td>@item.Name</td>  @* ✅ Correct property *@
```

### JavaScript File 404 Errors
```html
<!-- Problem: JavaScript files in Views folder aren't served -->
<script src="~/Areas/BusinessIncubators/Views/ProjectKnowledgeStructure/_EditFunctions.js"></script>  <!-- ❌ 404 -->

<!-- Solution: Move to wwwroot -->
<script src="~/js/businessincubators/projectknowledgestructure-edit.js"></script>  <!-- ✅ -->
```

## Tree View Issues

### Answer Options Not Displaying
```csharp
// Problem: Answer options not loaded
.Include(p => p.ProjectBlocks)
    .ThenInclude(b => b.ProjectQuestions)  // ❌ Missing answer options

// Solution: Include all nested entities
.Include(p => p.ProjectBlocks)
    .ThenInclude(b => b.ProjectQuestions)
        .ThenInclude(q => q.ProjectAnswerOptions)  // ✅
```

### Inconsistent Node ID References
```javascript
// Problem: ID might be in different properties
const id = node.data.id;  // ❌ May be undefined

// Solution: Use helper function
function getNodeEntityId(node) {
    if (!node || !node.data) return null;
    return node.data.entityId || node.data.id || null;  // ✅
}
```

### Duplicate Function Definitions
```javascript
// Problem: Same function in multiple files causes conflicts
// In main view:
function showEditDialog(node) { ... }  // ❌
// In external JS:
function showEditDialog(node) { ... }  // ❌

// Solution: Remove duplicates, keep single definition
// Only in external JS file  // ✅
```

### Missing Model Declarations
```razor
@* Problem: Model not declared *@
<h1>@Model.Title</h1>  @* ❌ Model is null *@

@* Solution: Declare model *@
@model ProjectViewModel
<h1>@Model.Title</h1>  @* ✅ *@
```

## StyleCop Quick Fixes

### Bulk Trailing Whitespace Removal
```bash
# Remove from all .cs files
sed -i 's/[[:space:]]*$//' **/*.cs
```

### Add Missing Newlines to Files
```csharp
// Use MultiEdit tool
edits: [
    { "old_string": "}", "new_string": "}\n" }
]
```

## Entity Framework Issues

### Include Missing Related Data
```csharp
// Problem: Navigation property is null
var project = await repository.GetByIdAsync(id);
var blocks = project.ProjectBlocks;  // ❌ Null

// Solution: Include related data
var project = await repository.GetWithProjectBlocksByExternalId(id);
var blocks = project.ProjectBlocks;  // ✅ Loaded
```

### Tracking Conflicts
```csharp
// Problem: Entity already tracked
dbContext.Update(entity);  // ❌ Tracking conflict

// Solution: Check tracking state
var tracked = dbContext.ChangeTracker.Entries<Project>()
    .FirstOrDefault(e => e.Entity.Id == entity.Id);
if (tracked != null)
{
    dbContext.Entry(tracked.Entity).CurrentValues.SetValues(entity);
}
```

## MediatR Command/Query Issues

### Handler Not Found
```csharp
// Problem: No handler registered
var result = await mediator.Send(command);  // ❌ Handler not found

// Solution: Ensure handler is registered in DI
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProjectCommand).Assembly));
```

### Wrong Return Type
```csharp
// Problem: Handler return type mismatch
public class Handler : IRequestHandler<Query, ProjectDto>  // ❌ Should return Result<T>

// Solution: Wrap in Result
public class Handler : IRequestHandler<Query, Result<ProjectDto>>  // ✅
```

## Database Issues

### NULL Values in Seed Data
**Error**: `Cannot insert the value NULL into column 'ProjectQuestionId'`
**Cause**: IF NOT EXISTS blocks prevent retrieving existing IDs
**Solution**: Add ELSE clauses to retrieve existing IDs
```sql
IF NOT EXISTS (SELECT 1 FROM [table] WHERE condition)
BEGIN
    INSERT INTO [table] ...
    SET @NewId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @NewId = Id FROM [table] WHERE condition;
END
```

### Duplicate Key Violation for Multi-Choice
**Error**: `Violation of UNIQUE constraint` when selecting multiple answers
**Cause**: Unique constraint doesn't account for AnswerOptionId
**Solution**: Include AnswerOptionId in unique constraint
```sql
CREATE UNIQUE INDEX [UQ_DiagnosisAnswers_ProjectId_UserId_QuestionId_AnswerOptionId_Phase] 
ON [diagnostics].[DiagnosisAnswers](
    [ProjectId], [UserId], [QuestionId], [AnswerOptionId], [Phase]
);
```

### Type Mismatch in Owned Collections
**Error**: `Expected type was 'System.Int32' but actual value was of type 'System.Int64'`
**Cause**: EF Core assuming Int32 for owned collection IDs
**Solution**: Explicitly configure as long
```csharp
entity.OwnsMany(x => x.PhaseSummaries, summaries =>
{
    summaries.Property<long>("Id");  // Explicitly configure as long
    summaries.HasKey("Id");
});
```

## Enum Parsing Issues

### Cross-Domain Enum Mapping
**Important**: Enums should match 1:1 between bounded contexts when representing the same concept.
- Diagnostics domain is the source of truth for FODA and ODSR types
- BusinessIncubator domain must use matching enum values

```csharp
// Both domains should have identical enums:
// FodaType: NoDefinido='N', Fortalezas='F', Oportunidades='O', Debilidades='D', Amenazas='A'
// OdsrType: NoDefinido='N', Ofensiva='O', Defensiva='D', Supervivencia='S', Reorientacion='R'
```

**Solution**: When enums match 1:1, use simple cast conversion
```csharp
private OdsrType ConvertToOdsrType(BusinessIncubatorEnums.OdsrType? odsrType)
{
    if (!odsrType.HasValue)
    {
        return OdsrType.NoDefinido;
    }

    // Enums match 1:1, just cast with same char values
    return (OdsrType)(char)odsrType.Value;
}
```

### FODA/ODSR String Conversion
```csharp
// Problem: Invalid enum value
var type = Enum.Parse<FODAType>(value);  // ❌ "F" is not valid

// Solution: Map single characters
var type = value switch
{
    "F" => FODAType.Fortaleza,
    "O" => FODAType.Oportunidad,
    "D" => FODAType.Debilidad,
    "A" => FODAType.Amenaza,
    _ => throw new ArgumentException($"Invalid FODA type: {value}")
};
```

## Database Migration Issues

### Permission Seeding
```sql
-- Problem: New controller actions not accessible
-- Solution: Update 001.SeedWebFeatures.sql
INSERT INTO [permissions].[ProtectedResources] 
    ([Id], [Name], [Url], [ResourceType]) 
VALUES 
    (NEWID(), 'Projects - Batch Upload', '/BusinessIncubators/Projects/BatchUserRegistration', 'WebFeature');
```

## Validation Issues

### Duplicate Prevention
```csharp
// At orchestration level
var uniqueUsers = users.DistinctBy(u => u.Email).ToList();

// At domain level
if (invitations.Any(i => i.Email == email))
{
    return Result.Fail("Usuario ya invitado");
}
```

## Navigation Link Redirection Pattern
When consolidating duplicate features to a unified interface:

```csharp
// In Views - redirect to unified interface across areas
// Old: Direct controller action in same area
Url = Url.Action("BatchUserRegistration", "Projects", new { businessIncubatorId, projectId })

// New: Cross-area redirection with explicit area parameter
Url = Url.Action("BulkInvite", "Participant", new { area = "Coordination", projectId })

// In hover actions/datatables - use direct URL format
new() { Url = $"/Coordination/Participant/BulkInvite?projectId={{externalId}}", IconClass = "fas fa-users", Text = "Registro masivo de usuarios" }
```

**Key Points:**
- Always specify `area` parameter when redirecting across areas
- Use direct URL format for DataTable hover actions with placeholder syntax `{{externalId}}`
- Update WebFeatures.sql to remove obsolete action entries

### CSV/Excel File Parsing for Bulk Operations

### Column Order for User Import
Standard column order for bulk user invitation files:
1. Email
2. FirstName
3. LastName
4. IdentificationNumber
5. PhoneNumber (optional)
6. Role (optional, uses default if empty)

### Excel Parsing with EPPlus
```csharp
// Skip header row, start from row 2
for (int row = 2; row <= rowCount; row++)
{
    var email = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? string.Empty;
    var firstName = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? string.Empty;
    var lastName = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? string.Empty;
    var identificationNumber = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? string.Empty;
    // Continue for other columns...
}
```

### CSV Parsing Pattern
```csharp
// Parse CSV with proper quote handling
var columns = line.Split(',');
invitations.Add(new InvitationData
{
    Email = columns[0].Trim().Trim('"'),
    FirstName = columns[1].Trim().Trim('"'),
    LastName = columns[2].Trim().Trim('"'),
    IdentificationNumber = columns.Length > 3 ? columns[3].Trim().Trim('"') : string.Empty,
    // Handle optional columns safely
});
```

## File Upload Validation
```csharp
// Size validation
if (file.Length > 10 * 1024 * 1024) // 10MB
{
    return Result.Fail("El archivo excede el tamaño máximo permitido");
}

// Type validation
var allowedExtensions = new[] { ".csv", ".xlsx" };
if (!allowedExtensions.Contains(Path.GetExtension(file.FileName)))
{
    return Result.Fail("Tipo de archivo no permitido");
}
```

## Many-to-Many Relationship Issues

### JavaScript Template String Error in DatatableTagHelper
```csharp
// Problem: Malformed template string for URLs without parameters
url = tokenMatches.Count > 0
    ? $"${{`{url}`.{replacementChain}}}"  // ❌ Missing closing brace
    : url;

// Solution: Correct template string syntax
url = tokenMatches.Count > 0
    ? $"${{`{url}`.{replacementChain}}}"
    : url;  // ✅ No template string needed when no tokens
```

### Missing Navigation Properties in Repository
```csharp
// Problem: NullReferenceException when accessing related entities
var topics = await dbContext.KnowledgeStructureTopics
    .Where(t => t.KnowledgeStructureModuleId == id)
    .ToListAsync();  // ❌ Topic property is null

// Solution: Include navigation properties
var topics = await dbContext.KnowledgeStructureTopics
    .Include(t => t.Topic)  // ✅ Load Topic
    .Include(t => t.KnowledgeStructureModule)
        .ThenInclude(m => m.Module)  // ✅ Load nested Module
    .Where(t => t.KnowledgeStructureModuleId == id)
    .ToListAsync();
```

### Parameter Type Mismatch in Controllers
```csharp
// Problem: Method expects string but receives long
public async Task<IActionResult> GetTreeData(long? id)  // ❌ Type mismatch

// Solution: Match parameter types with view expectations
public async Task<IActionResult> GetTreeData(string? id)  // ✅ Correct type
{
    if (string.IsNullOrEmpty(id) || !long.TryParse(id, out var structureId))
        return Json(new List<object>());
}
```

### Missing UI for Many-to-Many Management
```csharp
// Problem: Create forms only support single assignment
// Modules can belong to multiple knowledge structures but UI doesn't support it

// Solution: Add dedicated management views
[HttpGet]
public async Task<IActionResult> ManageKnowledgeStructures(long id)
{
    // Get module with all its relationships
    var query = new GetModuleKnowledgeStructuresQuery(id);
    var result = await mediator.SendAndLogIfFailureAsync(query);
    
    // Show view with checkboxes for assignment/removal
    return View(new ManageViewModel { /* ... */ });
}
```

## Test Infrastructure Issues

### Integration Tests Fail with Missing DACPAC
```bash
# Problem: Integration tests fail with .dacpac file not found
System.IO.FileNotFoundException : .dacpac file not found

# Solution 1: Build database project first
cd LindaDb
MSBuild LinaDb.sqlproj -p:Configuration=Debug

# Solution 2: Fix test setup to use correct path
// In DacpacDeployer.cs, verify path:
var dacpacPath = Path.Combine(Directory.GetCurrentDirectory(), 
    @"..\..\..\..\LindaDb\bin\Debug\LinaDb.dacpac");

# Solution 3: Skip integration tests temporarily
dotnet test --filter "Category!=Integration"
```

## JavaScript and UI Issues

### SweetAlert2 Not Available in Phoenix Template
**Error**: `ReferenceError: Swal is not defined`
**Cause**: Phoenix Admin template doesn't include SweetAlert2
**Solution**: Use Bootstrap modals instead
```javascript
// ❌ WRONG - SweetAlert2 not available
Swal.fire({ title: 'Confirm', ... });

// ✅ CORRECT - Use Bootstrap modals
function showConfirmModal(title, message, confirmText, cancelText) {
    return new Promise((resolve) => {
        const modalHtml = `
            <div class="modal fade" id="confirmModal">
                <!-- Bootstrap modal structure -->
            </div>
        `;
        // Append to body and show modal
    });
}
```

### Submit Button Visibility for Approved Forms
**Issue**: Submit button showing on approved form submissions
**Cause**: Action buttons container with `display: none !important;` overrides JavaScript
**Solution**: Remove !important and control visibility via JavaScript
```html
<!-- ❌ WRONG - !important prevents JavaScript control -->
<div id="actionButtons" style="display: none !important;">

<!-- ✅ CORRECT - Allow JavaScript to control -->
<div id="actionButtons">
```

```javascript
// Control button visibility based on form status
if (this.config.isReadOnly && !this.config.canSubmit) {
    // Hide entire footer for approved forms with single block
    if (this.formStructure.blocks.length <= 1) {
        actionButtons.style.display = 'none';
    }
}
```

## Entity Framework Core Configuration Issues

### ValueObject Requires Primary Key
```csharp
// Problem: The entity type 'DashboardPreferences' requires a primary key
// ValueObjects don't have keys but EF tries to map them as entities

// Solution: Configure as owned type in DbContext
modelBuilder.Entity<UserDashboard>(entity =>
{
    entity.OwnsOne(e => e.Preferences, p =>
    {
        p.Property(x => x.Theme).HasColumnName("PreferencesTheme");
        p.Property(x => x.Language).HasColumnName("PreferencesLanguage");
        // Map all properties as columns in parent table
    });
});
```

### No Backing Field for Computed Property
```csharp
// Problem: No backing field could be found for property 'CreatedDate'
// Computed properties without setters can't be mapped

// Entity with computed property:
public DateTime CreatedDate => CreatedAt; // No setter!

// Solution: Tell EF Core to ignore it
modelBuilder.Entity<UserNotification>(entity =>
{
    entity.Ignore(e => e.CreatedDate); // Ignore computed property
    entity.Property(e => e.CreatedAt).IsRequired(); // Map actual property
});
```

### Audit Fields Not Being Set
```csharp
// Problem: Cannot insert NULL into column 'CreatedBy'
// Audit fields need to be set automatically

// Solution: Override SaveChangesAsync in DbContext with IAuditContext
public class CoreDbContext : DbContext
{
    private readonly IAuditContext? _auditContext;
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateAuditFields()
    {
        if (_auditContext == null) return;
        
        var entries = ChangeTracker.Entries<YourEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = _auditContext.UtcNow;
                entry.Property("CreatedBy").CurrentValue = _auditContext.User ?? "system";
            }
        }
    }
}
```

### Enum Conversion Errors
```csharp
// Problem: Unable to cast object of type 'System.String' to type 'System.Int32'
// Enums stored differently in database vs domain model

// Solution: Configure explicit enum conversions
modelBuilder.Entity<UserNotification>(entity =>
{
    // For enums stored as strings in database
    entity.Property(e => e.Type)
        .HasConversion<string>();
    
    // For enums stored as integers in database
    entity.Property(e => e.Priority)
        .HasConversion<int>();
});
```

### Column Name Mismatches
```csharp
// Problem: Invalid column name 'Code', 'RoleIds', etc.
// Domain properties don't match database column names

// Solution: Map properties to actual column names
modelBuilder.Entity<DashboardWidget>(entity =>
{
    entity.Property(e => e.Name).HasColumnName("Name");
    entity.Property(e => e.RoleIds).HasColumnName("Roles");
    entity.Property(e => e.Configuration).HasColumnName("DefaultConfig");
    
    // Ignore properties that don't exist in database
    entity.Ignore(e => e.WidgetId);
    entity.Ignore(e => e.GridRow);
    
    // Add shadow properties for database columns not in domain
    entity.Property<string>("DisplayName").HasMaxLength(200);
});
```

### Database Schema Mismatch
```bash
# Problem: Invalid column name errors after domain changes
# Entity properties don't match database columns

# Solution 1: Update SQL table definition
# In LindaDb/core/Tables/*.sql, add missing columns

# Solution 2: Build and deploy database project
cd LindaDb
MSBuild LinaDb.sqlproj -p:Configuration=Debug
SqlPackage.exe /Action:Publish /SourceFile:"bin\Debug\LinaDb.dacpac" \
  /TargetConnectionString:"..." /Properties:BlockOnPossibleDataLoss=false

# Solution 3: Migrate existing data if needed
sqlcmd -Q "UPDATE [table] SET [NewColumn] = [OldColumn]"
```

## MediatorExecutor Pattern Issues

### Problem: Controllers using IMediator directly
```csharp
// ❌ WRONG - Never use IMediator directly in controllers
public class MyController(IMediator mediator) : Controller
{
    var result = await mediator.Send(command);
}
```

### Solution: Use MediatorExecutor with IBaseRequest
```csharp
// ✅ CORRECT - Use MediatorExecutor with BaseController
public class MyController(
    ILogger<MyController> logger,
    MediatorExecutor mediatorExecutor) : BaseController(logger, mediatorExecutor)
{
    var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
    if (!result.IsSuccess)
    {
        this.MapErrorsToModelStateAndSetErrorToast<CommandType>(result);
        return RedirectToAction("Index");
    }
}
```

### Command/Query Implementation
```csharp
// ❌ WRONG - Using IRequest directly
public record MyQuery(Guid Id) : IRequest<Result<MyDto>>;

// ✅ CORRECT - Using IBaseRequest for MediatorExecutor
public record MyQuery(Guid Id) : IBaseRequest<MyDto>;
```

### Compilation Errors
```
CS1503: cannot convert from 'MyCommand' to 'IBaseRequest'
Solution: Change command/query to implement IBaseRequest<T> instead of IRequest<Result<T>>
```

## Result Pattern Implementation

### Critical Pattern Error
```csharp
// ❌ WRONG - Double-wrapping Result
public record UpdateCommand(...) : IBaseRequest<Result>;
public class UpdateCommandHandler : BaseCommandHandler<UpdateCommand, Result>
{
    public override async Task<Result<Result>> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        return Success(Result.Success()); // WRONG: Nested Result!
    }
}
```

### Correct Result Pattern Usage
```csharp
// ✅ CORRECT - Command without return value
public record UpdateCommand(...) : IBaseRequest; // No type parameter!
public class UpdateCommandHandler : BaseCommandHandler<UpdateCommand> // No second type!
{
    public override async Task<Result> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        // ... logic
        return Success(); // Just Success(), not Success(Result.Success())
    }
}

// ✅ CORRECT - Query with return value
public record GetQuery(...) : IBaseRequest<MyDto>; // Return type only
public class GetQueryHandler : BaseCommandHandler<GetQuery, MyDto>
{
    public override async Task<Result<MyDto>> Handle(GetQuery request, CancellationToken cancellationToken)
    {
        var dto = new MyDto { /* ... */ };
        return Success(dto); // Success with value
    }
}
```

### Key Rules
1. **IBaseRequest** (no param) → Handler returns **Result**, use **Success()** or **Failure(...)**
2. **IBaseRequest<T>** → Handler returns **Result<T>**, use **Success(value)** or **Failure(...)**
3. **Never nest Results**: IBaseRequest already implies Result wrapping
4. **Handler inheritance**: BaseCommandHandler<TCommand> for no return, BaseCommandHandler<TCommand, TResult> for return

### Namespace Ambiguity
```csharp
// Problem: Ambiguous between MediatR.IBaseRequest and LinaSys.Shared.Application.MediatR.IBaseRequest
// Solution: Use fully qualified namespace
public record MyCommand(...) : LinaSys.Shared.Application.MediatR.IBaseRequest;
```

## Pagination Pattern

### Use Shared FilteredQueryResult
```csharp
// ❌ WRONG - Creating custom pagination DTOs
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    // ... custom implementation
}

// ✅ CORRECT - Use shared FilteredQueryResult for DataTables
using LinaSys.Shared.Application;

public record ListQuery(
    int Start = 0,           // DataTable convention
    int Length = 10,         // DataTable convention
    string? SearchTerm = null) : IBaseRequest<FilteredQueryResult<MyDto>>;

// In handler:
var recordsTotal = await baseQuery.CountAsync(cancellationToken);
var recordsFiltered = await filteredQuery.CountAsync(cancellationToken);
var items = await filteredQuery
    .Skip(request.Start)
    .Take(request.Length)
    .Select(x => new MyDto { /* ... */ })
    .ToListAsync(cancellationToken);

return Success(FilteredQueryResult.From(items, recordsTotal, recordsFiltered));
```

### Key Points
1. **Always use FilteredQueryResult<T>** from LinaSys.Shared.Application for paginated results
2. **Follow DataTable conventions**: Use Start/Length, not PageNumber/PageSize
3. **Calculate both counts**: recordsTotal (all records) and recordsFiltered (after filters)
4. **Use factory method**: FilteredQueryResult.From(items, total, filtered)

## Cross-Domain Data Integration

### Placeholder Pattern for Missing Cross-Domain Data
When data from another bounded context is not yet available, use placeholders with TODOs:

```csharp
// ❌ WRONG - Accessing non-existent properties causes build errors
Email = u.Email ?? string.Empty,  // UserProfileDto doesn't have Email

// ✅ CORRECT - Use placeholder with clear TODO
Email = string.Empty, // TODO: Get email from Auth domain

// For audit fields not yet in entity
CreatedDate = DateTime.UtcNow, // TODO: Add audit fields to UserProfile
UpdatedDate = null // TODO: Add audit fields to UserProfile
```

### Key Points
1. **Document missing integration**: Always add TODO comments explaining what's needed
2. **Use safe defaults**: string.Empty for strings, DateTime.UtcNow for required dates, null for optional
3. **Track in session notes**: Add to "Blocking Issues" in CURRENT_SESSION.md
4. **Plan for resolution**: Add to "Next Session Priority" for implementation

## Azure Storage and Aspire Integration

### Package Dependencies
When using Azure Blob Storage with Aspire, ensure all required packages are present:

```xml
<!-- In Directory.Packages.props for central package management -->
<PackageVersion Include="Aspire.Azure.Storage.Blobs" Version="9.4.1" />
<PackageVersion Include="Azure.Storage.Blobs" Version="12.25.0" />
<PackageVersion Include="Microsoft.Extensions.Configuration.Binder" Version="9.0.0" />
<PackageVersion Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.0" />
```

### Obsolete Method Warning
```csharp
// ❌ WRONG - Obsolete method
builder.AddAzureBlobClient("blobs");

// ✅ CORRECT - Use the new method
builder.AddAzureBlobServiceClient("blobs");
```

### Dictionary Extension Methods
.NET 9 IDictionary doesn't have GetValueOrDefault:

```csharp
// ❌ WRONG - GetValueOrDefault not available on IDictionary
var value = metadata.GetValueOrDefault("key", "default");

// ✅ CORRECT - Use TryGetValue pattern
var value = metadata.TryGetValue("key", out var val) ? val : "default";
```

### StyleCop Compliance
With `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`:

```csharp
// ❌ WRONG - Missing braces (SA1503)
if (condition)
    return value;

// ✅ CORRECT - Always use braces
if (condition)
{
    return value;
}

// ❌ WRONG - Tuple element names in camelCase (SA1316)
private (string containerName, string blobName) ExtractNames()

// ✅ CORRECT - Use PascalCase for tuple elements
private (string ContainerName, string BlobName) ExtractNames()
```

### Dependency Injection Registration
For shared infrastructure services:

```csharp
// In Program.cs
builder.Services.AddSharedInfrastructureServices(builder.Configuration);

// In Shared.Infrastructure/DependencyInjection.cs
public static void AddSharedInfrastructureServices(this IHostApplicationBuilder builder)
{
    builder.AddAzureBlobServiceClient("blobs");
    builder.Services.AddSingleton<IFileStorageService, AzureBlobFileStorageService>();
}
```

## Clean Architecture - Web Layer Restrictions
**Issue**: CS0234 - Web project cannot reference Domain projects directly  
**Example**: `UserManagement.Domain.Constants` not accessible from Web layer

**Solution**:
```csharp
// ❌ WRONG - Web referencing Domain
emailPreferences[UserManagement.Domain.Constants.PreferenceKeys.EmailSystemWelcome] = "true";

// ✅ CORRECT - Use string literals in Web layer
emailPreferences["email.system.welcome"] = "true";

// ✅ ALTERNATIVE - Define in Shared.Domain if cross-cutting
// LinaSys.Shared.Domain.Constants.PreferenceKeys
```

**Pattern**: Keep domain constants internal, expose through Application/Orchestration layers

## Existing Infrastructure - Don't Recreate

### User Management Infrastructure Already Present
When implementing user management features, check what already exists:

```csharp
// ✅ EXISTING - UserPreferences entity (key-value storage)
UserManagement.Domain.AggregatesModel.UserProfileAggregate.UserPreferences
// Use: userProfile.AddOrUpdatePreference(key, value, auditContext)

// ✅ EXISTING - UserActivities table (comprehensive audit)
[core].[UserActivities] // Type, Category, Action, EntityType, EntityId, Metadata (JSON)

// ✅ EXISTING - UserNotifications table (in-app notifications)  
[core].[UserNotifications] // Type, Priority, Title, Message, IsRead, ExpiresAt

// ✅ EXISTING - ProjectUser entity (project-level roles)
BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectUser
// Handles: UserId, Role, IsActive, JoinedAt, LeftAt, InvitedBy

// ✅ EXISTING - UserProjectAccess (read model for fast access checks)
Auth.Domain.AggregatesModel.Access.UserProjectAccess
// Synchronized via integration events

// ✅ EXISTING - IEmailTemplateService (11 email templates)
Notification.Application.Templates.IEmailTemplateService
// Methods for: Welcome, Invitation, Approval, Rejection, etc.

// ✅ EXISTING - ProjectInvitations (with 72h token expiry)
BusinessIncubator.Domain.Aggregates.BusinessIncubator.ProjectInvitation
```

### Key Pattern: Don't Create Redundant Backend
```csharp
// ❌ WRONG - Creating new preference system
public class EmailPreferences : Entity
{
    public string UserId { get; set; }
    public bool SendWelcomeEmail { get; set; }
}

// ✅ CORRECT - Use existing UserPreferences with keys
public static class PreferenceKeys
{
    public const string EmailSystemWelcome = "email.system.welcome";
    public const string EmailProjectWelcome = "email.project.welcome";
}
// Then: userProfile.AddOrUpdatePreference(PreferenceKeys.EmailSystemWelcome, "true", auditContext)
```
### Namespace Conflicts

**Issue**: Folder name conflicts with class name causing CS0118 'is a namespace but is used like a type'

**Example**: Notification.Infrastructure/Email/ folder conflicting with Email class

**Solution**: Use fully qualified names for the class
```csharp
// Instead of:
private readonly ConcurrentQueue<Email> _emailQueue = new();

// Use:
private readonly ConcurrentQueue<Domain.AggregatesModel.Email> _emailQueue = new();
```

**Alternative**: Rename the folder to avoid the conflict (e.g., Email -> EmailServices)

## Integration Events

### Missing Data in Integration Events
**Issue**: Events published without complete information needed by handlers
**Solution**: Enrich events at source with all needed data

**Example**: ReviewChangesRequestedIntegrationEvent
```csharp
// ❌ WRONG - Missing critical information
public record ReviewChangesRequestedIntegrationEvent(
    long SubmissionId, string Feedback);

// ✅ CORRECT - Complete information at source
public record ReviewChangesRequestedIntegrationEvent(
    long SubmissionId,
    long ProjectId,
    string ProjectName,
    string ParticipantUserId,
    string ParticipantName,
    string ParticipantEmail,
    string ReviewerName,
    string Feedback,
    DateTime? NewDeadline);
```

**Pattern**: Fetch all necessary data in command handler before publishing event

## StyleCop Analyzer Issues in Razor Pages

### SA1201: Element Ordering
**Issue**: "A method should not follow a class"
**Solution**: In Razor Pages, place nested classes at the end of the file
```csharp
public class PageModel
{
    // Properties first
    public string Property { get; set; }
    
    // Static methods before instance methods
    private static bool Helper() { }
    
    // Instance methods
    public async Task OnGetAsync() { }
    
    // Nested classes last
    public class InputModel { }
}
```

### SA1204: Static Members Ordering
**Issue**: "Static members should appear before non-static members"
**Solution**: Order members as: static fields, static properties, static methods, then instance members

### SA1513: Closing Brace Formatting
**Issue**: "Closing brace should be followed by blank line"
**Solution**: Add blank line after closing braces except when followed by another closing brace

## Repository Pattern Violations

### Orchestration Layer Using Repositories Directly
**Issue**: Orchestration commands directly injecting repositories instead of using MediatR
**Cause**: Violates bounded context separation

**Wrong Pattern**:
```csharp
public class CreateUserWithProfileOrchestrationCommandHandler(
    IUserProfileRepository userProfileRepository) // ❌ Direct repository injection
{
    var userProfile = UserProfile.Create(...);
    userProfileRepository.Add(userProfile);
}
```

**Correct Pattern**:
```csharp
public class CreateUserWithProfileOrchestrationCommandHandler(
    IMediator mediator) // ✅ Use MediatR for cross-boundary
{
    var command = new CreateUserProfileCommand(...);
    var result = await mediator.Send(command);
}
```

## Unused Infrastructure Discovery

### AuditService Not Integrated
**Finding**: Complete audit infrastructure exists but no command handlers call it
**Location**: Core.Infrastructure/Services/AuditService.cs
**Methods**: LogCreateAsync, LogUpdateAsync, LogDeleteAsync, LogActionAsync
**Recommendation**: Future enhancement - integrate audit logging in critical command handlers

## Email Infrastructure Issues

### EmailTemplate Entity ID Type Mismatch
**Error**: `Unable to cast object of type 'System.Guid' to type 'System.Int64'`
**Cause**: EmailTemplates table using UNIQUEIDENTIFIER while Entity base class expects BIGINT
**Solution**: Change table to use BIGINT IDENTITY(1,1) for consistency with all other domain tables

```sql
-- ❌ WRONG - Inconsistent with other tables
[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()

-- ✅ CORRECT - Consistent with system pattern
[Id] BIGINT IDENTITY(1,1) NOT NULL
```

### Mailgun Sandbox Recipient Restrictions
**Error**: 401 Unauthorized (misleading error message)
**Actual Issue**: Sandbox domains require authorized recipients
**Diagnosis**: Check Mailgun logs - if `"is-authenticated": true` but `"event": "rejected"`
**Solution**: Add recipients as authorized in Mailgun dashboard

```csharp
// Enhanced error handling for clearer diagnostics
if (respBody.Contains("Sandbox subdomains are for test purposes only"))
{
    throw new InvalidOperationException($"Mailgun sandbox restriction: Add '{email.To}' as an authorized recipient");
}
```

### Testing Email Infrastructure
**Quick Test Command**: `dotnet run --project Sandbox -- mailgun`
**Integration Tests**: Tests.UnitTesting/Integration/Modules/Notification/Infrastructure/Email/
**Configuration**: Ensure Domain, ApiKey, and FromAddress match Mailgun account settings

## Entity Framework Core with DDD Private Collections

### Include with Private Backing Fields
**Error**: `The expression 'p.ProjectStages' is invalid inside an 'Include' operation`
**Cause**: EF Core cannot use Include with public read-only collection properties that have private backing fields
**Solution**: Use string-based Include with the private field name

```csharp
// Domain entity with encapsulation
public class Project : Entity
{
    private readonly List<ProjectStage> _projectStages = new();
    public IReadOnlyCollection<ProjectStage> ProjectStages => _projectStages.AsReadOnly();
}

// ❌ WRONG - Include with public property fails
.Include(p => p.ProjectStages)

// ✅ CORRECT - Include with private field name
.Include("_projectStages")
```

## Form Discovery Issues

### Dashboard Shows All Projects Instead of Selected One
**Issue**: Participant Dashboard ignores selected project context
**Cause**: Not using CurrentUserContext.ProjectId from context selection

```csharp
// ❌ WRONG - Gets all projects for user
var projectsQuery = new GetParticipantProjectsQuery(userId);

// ✅ CORRECT - Use selected project from context
var context = DemandCurrentUserContext(requireProject: true);
var projectId = context.ProjectId!.Value;
var projectQuery = new GetProjectDetailsQuery(projectId);
```

### Forms Not Visible Until Created
**Issue**: Dashboard only shows existing forms, not available ones
**Cause**: System uses lazy form creation without discovery mechanism
**Solution**: Create GetAvailableFormsQuery that checks active stages

```csharp
// Check active stages for available forms
foreach (var stage in project.ProjectStages.Where(s => s.IsActive))
{
    if (stage.Type == ProjectStageType.InitialFormCollection || 
        stage.Type == ProjectStageType.FinalFormCollection)
    {
        var phase = ProjectFormSubmission.GetPhaseForStage(stage.Type);
        var existingForm = await GetExistingForm(userId, projectId, phase);
        
        // Show form as available even if not created
        availableForms.Add(new AvailableFormDto
        {
            IsCreated = existingForm != null,
            Status = existingForm?.Status,
            CanStart = stage.IsWithinPeriod(currentDate)
        });
    }
}

## Form System Issues

### Textarea Values Not Saving in Drafts
**Issue**: Textarea form fields not being collected when saving drafts
**Cause**: JavaScript querying for wrong data attribute - looking for `data-answer-type="text"` but textareas have `data-answer-type="textarea"`

**Solution**: Separate case for FreeText questions
```javascript
// ❌ WRONG - Only queries for input elements
case 3: // FreeText
    const answerElement = document.querySelector(`[data-question-id="${question.id}"][data-answer-type="text"]`);

// ✅ CORRECT - Query specifically for textarea
case 3: // FreeText
    const answerElement = document.querySelector(`textarea[data-question-id="${question.id}"]`);
    if (answerElement && answerElement.value) {
        answer.value = answerElement.value;
        answer.isAnswered = true;
    }
```

### Dashboard Showing Incorrect Completion Percentage
**Issue**: Dashboard shows 100% when only partial questions answered (e.g., 5/5 = 100% instead of 5/15 = 33%)
**Cause**: Using response count as total instead of actual question count from structure

**Solution**: Always load project structure to get accurate totals
```csharp
// ❌ WRONG - Using response count as total
int totalQuestions = request.DraftData.BlockResponses.Count;

// ✅ CORRECT - Get total from project structure
if (!project.ProjectBlocks.Any())
{
    project = await repository.GetProjectWithBlocksByIdAsync(project.Id, cancellationToken);
}
int totalQuestions = project.ProjectBlocks.Sum(b => b.ProjectQuestions?.Count ?? 0);
int answeredQuestions = request.DraftData.BlockResponses
    .Sum(block => block.QuestionResponses.Count(q => q.IsAnswered));
```

### Real-time UI Not Updating
**Issue**: Progress bars and completion checkmarks only update on save/navigation
**Cause**: Missing event handlers for field changes

**Solution**: Add real-time updates on any field change
```javascript
// Add to all input handlers
onFieldChange() {
    // Save immediately
    window.formManager.saveCurrentBlock();
    
    // Update all UI elements in real-time
    window.formManager.updateProgressPercentage();
    window.formManager.updateProgress();
    window.formManager.updateBlockCompletionStatus(window.formManager.currentBlockIndex);
}

// Attach to form fields
$(document).on('change', 'input, select, textarea', function() {
    window.formManager.onFieldChange();
});
```

### Wizard Tab Navigation Bypassing Validation
**Issue**: Users can click wizard tabs to skip validation
**Cause**: Tab click handler not checking validation state

**Solution**: Validate only for forward navigation
```javascript
$(document).on('click', '.wizard-tab', function(e) {
    const targetIndex = parseInt($(this).data('block-index'));
    const currentIndex = window.formManager.currentBlockIndex;
    
    // Only validate when moving forward
    if (targetIndex > currentIndex) {
        if (!window.formManager.validateCurrentBlock()) {
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
    }
    
    window.formManager.navigateToBlock(targetIndex);
});
```

### Case Conversion Between JavaScript and C#
**Issue**: JavaScript uses camelCase but C# uses PascalCase causing property mismatches
**Cause**: Default JSON serialization doesn't convert cases

**Solution**: Use JsonNamingPolicy.CamelCase
```csharp
// In Razor view for JavaScript consumption
let draftData = @Html.Raw(Model.DraftData != null ? 
    System.Text.Json.JsonSerializer.Serialize(Model.DraftData, 
        new System.Text.Json.JsonSerializerOptions { 
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
        }) : "null");

// In C# when deserializing from JavaScript
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
var draftData = JsonSerializer.Deserialize<DraftDataDto>(json, options);
```

## Integration Event Patterns

### Enriching Events with User Details
**Issue**: Integration events need user email/name but only have IDs
**Pattern**: Enrich event at source rather than in handler

```csharp
// ❌ WRONG - Handler queries for user details (cross-boundary)
public async Task Handle(ProjectStageActivatedIntegrationEvent notification, ...)
{
    foreach (var userId in notification.ParticipantUserIds)
    {
        var user = await userManager.FindByIdAsync(userId); // Cross-boundary query
        // ...
    }
}

// ✅ CORRECT - Event carries all needed data
// In command handler that publishes event:
var participants = new List<ParticipantNotificationInfo>();
foreach (var projectUser in projectWithUsers.ProjectUsers)
{
    var userResult = await mediator.Send(new GetUserByIdQuery(projectUser.UserId));
    participants.Add(new ParticipantNotificationInfo(
        UserId: projectUser.UserId,
        Email: userResult.Value.Email,
        FullName: userResult.Value.FullName));
}
var integrationEvent = new ProjectStageActivatedIntegrationEvent(
    Participants: participants, // Enriched with email/name
    // ...
);
```

### Notification Handler Consistency
**Pattern**: All notification handlers should use same services

```csharp
// ✅ CONSISTENT PATTERN
public class ProjectStageActivatedIntegrationEventHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,  // Use template service
    IApplicationUrlService applicationUrlService,  // Use URL service
    ILogger<...> logger)
{
    // Generate email using template service
    var emailBody = _emailTemplateService.GenerateProjectStageActivatedEmail(...);
    
    // Use application URL service for links
    var dashboardUrl = _applicationUrlService.GetParticipantProjectDashboardUrl(projectId);
}
```

## Domain Entity Property Mismatches

### ProjectFormSubmission Properties
**Common Mistakes**: Using properties from AuditableEntity when entity only inherits from Entity
```csharp
// ❌ WRONG - These properties don't exist
CreatedAt, UpdatedAt  // Entity doesn't inherit from AuditableEntity
IsDeleted            // Not available on this entity
CreatedBy            // Use ParticipantUserId instead

// ✅ CORRECT - Available properties
StartedAt            // When form was started
SubmittedAt          // When form was submitted (nullable)
ApprovedAt           // When form was approved (nullable)
RejectedAt           // When form was rejected (nullable)
ParticipantUserId    // The user who owns the form
```

### Project Entity Navigation Properties
**Issue**: Trying to access non-existent navigation properties
```csharp
// ❌ WRONG - Project doesn't have ProjectUsers navigation
var hasAccess = userProjects.Any(p => 
    p.ProjectUsers.Any(pu => pu.UserId == userId));

// ✅ CORRECT - GetProjectsByUserAsync already filters accessible projects
var hasAccess = userProjects.Any(p => p.Id == request.ProjectId.Value);

// Note: ProjectUsers is a separate table, not a navigation property on Project
// Access is managed through repository methods that query the ProjectUsers table
```

### ProjectStage Properties
**Note**: ProjectStage inherits from AuditableEntity
```csharp
// Available from AuditableEntity
CreatedAt, UpdatedAt, CreatedBy, UpdatedBy

// Domain-specific properties
EndDate  // DateTime (not nullable) - common mistake: treating as DateTime?
Type     // ProjectStageType enum
IsActive // bool

// ❌ WRONG - Property doesn't exist
OrderIndex  // Use Id for ordering if needed
```

## Build Configuration with StyleCop

### TreatWarningsAsErrors Configuration
**Configuration**: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
**Impact**: All StyleCop warnings become build errors

**Common Violations**:
- SA1028: Trailing whitespace
- SA1513: Closing brace needs blank line after
- SA1633: File must have copyright header
- SA1101: Prefix local calls with this

**Quick Build for Testing**:
```bash
# Bypass StyleCop errors temporarily
dotnet build -p:TreatWarningsAsErrors=false
```

### Namespace Conflicts in Views
**Issue**: Using enums from different namespaces in Razor views
```razor
@* ❌ WRONG - Full namespace in switch statements *@
case BusinessIncubator.Domain.Enums.ProjectFormSubmissionStatus.Draft:

@* ✅ CORRECT - Add using directive at top *@
@using LinaSys.BusinessIncubator.Domain.Enums
case ProjectFormSubmissionStatus.Draft:
```