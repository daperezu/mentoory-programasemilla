# Enhanced User Creation with Role-Based Access Assignment

> **Priority**: P1  
> **Module**: Coordination/UserManagement  
> **Estimate**: Large  
> **Status**: Pending  
> **Branch**: feature/enhanced-user-creation  

## Summary
Transform the current plain user creation form into a comprehensive, modern user onboarding experience with mandatory role selection and conditional incubator/project assignment based on the selected role.

## Business Context
Currently, users are created without roles or initial access permissions, requiring administrators to perform multiple separate actions to fully configure a new user. This creates operational inefficiency and can lead to users being created without proper access, preventing them from using the system effectively. The current UI is also basic and doesn't provide a good user experience.

## Acceptance Criteria
- [ ] User creation form requires role selection before user can be created
- [ ] Incubator selection becomes required/optional based on selected role
- [ ] Project selection becomes required/optional based on selected role
- [ ] Modern, intuitive UI with visual feedback and animations
- [ ] User is created with role and access permissions in a single transaction
- [ ] Proper validation messages in Spanish for all scenarios
- [ ] Success creates user with: account, profile, role, and access assignments
- [ ] Failure rolls back all operations maintaining data consistency
- [ ] Mobile-responsive design works on all screen sizes

## Technical Requirements

### Domain Layer
No changes required - existing domain entities support the operations:
- `UserIncubatorAccess` entity (Auth.Domain)
- `UserProjectAccess` entity (Auth.Domain)
- Existing validation rules remain

### Application Layer

#### Existing Commands to Use:
- `CreateUserWithProfileOrchestrationCommand` - Creates user and profile
- `AssignRolesToUserOrchestrationCommand` - Assigns roles to user
- `AssignUserToIncubatorCommand` - Grants incubator access
- `AssignUserToProjectCommand` - Grants project access

#### New Orchestration Command:
**`CreateUserWithRoleAndAccessOrchestrationCommand`**
```csharp
public record CreateUserWithRoleAndAccessOrchestrationCommand(
    // User Information
    string Email,
    string FirstName,
    string LastName,
    string Identification,
    string Password,
    
    // Role and Access
    string Role,
    long? IncubatorId,
    long? ProjectId,
    
    // Location (optional)
    string? Country,
    string? Province,
    string? Canton,
    string? District,
    string? FullAddress,
    
    // Preferences
    Dictionary<string, string>? EmailPreferences,
    bool EmailConfirmed,
    bool IsTemporaryPassword
) : IBaseRequest<UserCreationResultDto>;
```

**Validation Rules:**
- GlobalAdministrator: IncubatorId optional, ProjectId optional
- Administrator: IncubatorId required, ProjectId optional
- Coordinator: IncubatorId required, ProjectId optional
- All other roles: IncubatorId required, ProjectId required

#### Queries to Use:
- `GetEnrichedUserActiveIncubatorsQuery` - Load user's incubators
- `GetEnrichedUserProjectsQuery` - Load projects for selected incubator
- `GetAllIncubatorsQuery` - For GlobalAdministrator role
- `GetAllProjectsForIncubatorQuery` - Load all projects in incubator

### Infrastructure Layer
No changes required - existing repositories support all operations

### Web Layer

#### Enhanced ViewModel:
**`CreateUserViewModel.cs`** - Update existing model
```csharp
public class CreateUserViewModel
{
    // Existing properties remain...
    
    // New Role and Access Properties
    [Required(ErrorMessage = "El rol es requerido")]
    [Display(Name = "Rol")]
    public string SelectedRole { get; set; } = string.Empty;
    
    [Display(Name = "Incubadora")]
    public long? SelectedIncubatorId { get; set; }
    
    [Display(Name = "Proyecto")]
    public long? SelectedProjectId { get; set; }
    
    // Collections for dropdowns
    public List<RoleViewModel> AvailableRoles { get; set; } = new();
    public List<IncubatorViewModel> AvailableIncubators { get; set; } = new();
    public List<ProjectViewModel> AvailableProjects { get; set; } = new();
}

public class RoleViewModel
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string BadgeClass { get; set; } // For visual styling
    public bool RequiresIncubator { get; set; }
    public bool RequiresProject { get; set; }
}
```

#### Controller Updates:
**`UserManagementController.cs`** - Update Create actions

GET Action:
1. Load available roles based on current user permissions
2. Pre-populate incubators if user has limited access
3. Initialize enhanced view model
4. Return view with prepared data

POST Action:
1. Validate role-based requirements
2. Call new orchestration command
3. Handle success/failure with proper feedback
4. Redirect to user details on success

#### AJAX Endpoints:
```csharp
[HttpGet]
public async Task<IActionResult> GetIncubatorsForRole(string role)

[HttpGet]  
public async Task<IActionResult> GetProjectsForIncubator(long incubatorId, string role)
```

## UI/UX Requirements

### Visual Design:
1. **Card-based wizard interface** with clear sections:
   - Section 1: Información Personal (Name, Email, ID)
   - Section 2: Rol y Acceso (Role, Incubator, Project)
   - Section 3: Seguridad (Password, Email confirmation)
   - Section 4: Ubicación (Location - collapsible)
   - Section 5: Preferencias (Email preferences - collapsible)

2. **Role Selection Card Design:**
   - Visual cards with icons for each role
   - Role name with badge styling
   - Brief description of role responsibilities
   - Visual indicator of access requirements (chips showing "Requiere Incubadora", "Requiere Proyecto")

3. **Dynamic Form Behavior:**
   - Incubator/Project fields show/hide based on role
   - Loading spinners during AJAX calls
   - Real-time validation feedback
   - Smooth transitions between sections

4. **Visual Enhancements:**
   - Phoenix Admin Template components
   - Icons for each section (Font Awesome/Tabler Icons)
   - Progress indicator showing completion
   - Success animation on creation
   - Better spacing and typography

### JavaScript Implementation:
**New file: `/wwwroot/js/coordination/user-creation.js`**

Key features:
- Role selection handler with dynamic requirements
- AJAX loading of incubators/projects
- Form validation with visual feedback
- Section collapse/expand animations
- Password strength indicator
- Auto-save draft functionality

### Responsive Design:
- Mobile: Single column, collapsible sections
- Tablet: Two-column layout for form fields
- Desktop: Full wizard layout with sidebar progress

## Database Changes
No database changes required - using existing tables and relationships

## Security Considerations
- Role assignment restricted by current user's permissions
- Incubator/Project access validated against user's scope
- Password complexity enforced
- CSRF protection on forms
- Audit trail for all assignments

## Testing Requirements

### Unit Tests:
- Validation rules for role-based requirements
- Orchestration command logic
- Role permission checks

### Integration Tests:
- Complete user creation flow
- Transaction rollback on failure
- Access assignment verification

### E2E Scenarios:
1. Create user with GlobalAdministrator role (no access required)
2. Create user with Administrator role (incubator required)
3. Create user with Starter role (incubator and project required)
4. Validation failure scenarios
5. Transaction rollback on partial failure

## Implementation Notes

### Order of Implementation:
1. Create new orchestration command with validation
2. Update ViewModel with new properties
3. Update Controller GET action to load data
4. Create new UI with enhanced design
5. Implement JavaScript for dynamic behavior
6. Update Controller POST action to use new command
7. Add AJAX endpoints for dynamic loading
8. Test all scenarios end-to-end

### Key Considerations:
- Use existing queries and commands where possible
- Maintain transaction consistency
- Follow Clean Architecture principles
- All UI text in Spanish
- Use Phoenix Admin Template components only
- JavaScript in `/wwwroot/js/` folder

### Error Handling:
- Specific error messages for each validation scenario
- Rollback strategy if any step fails
- User-friendly error display with toast notifications

## Definition of Done
- [ ] Code implemented following Clean Architecture
- [ ] All tests written and passing
- [ ] StyleCop compliance (0 warnings, 0 errors)
- [ ] UI responsive on all screen sizes
- [ ] Spanish validation messages
- [ ] Transaction rollback tested
- [ ] Documentation updated
- [ ] WebFeatures.sql updated for new actions
- [ ] Code reviewed and approved

## Follow-up Tasks
- Add bulk user creation with CSV/Excel import including roles and access
- Add user templates for common role/access combinations
- Add ability to copy user settings from existing user
- Implement approval workflow for certain role assignments
- Add dashboard showing user creation statistics