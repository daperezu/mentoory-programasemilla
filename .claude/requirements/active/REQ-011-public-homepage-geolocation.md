# REQ-011: Public Homepage with Geolocation-Based Project Discovery

> **Priority**: P1  
> **Module**: BusinessIncubator / Web  
> **Estimate**: Large  
> **Status**: Pending  
> **Branch**: feature/public-homepage-geolocation  

## Summary
Create a public-facing marketing homepage that uses geolocation to show nearby incubator projects, allows visitors to express interest, and sends notification reminders before registration deadlines.

## Business Context
Currently, LinaSys requires authentication to view any projects, limiting discoverability for potential entrepreneurs. This feature will:
- Increase project visibility through a public homepage
- Help entrepreneurs find nearby incubators using location services
- Allow lightweight engagement through "interest expression" before full registration
- Improve conversion rates with timely notification reminders
- Support a new "Observer" role for users who only want to track projects

## Acceptance Criteria
- [ ] Public homepage accessible without authentication
- [ ] Geolocation prompt on first visit with explicit consent
- [ ] Projects displayed within 15km radius when location is shared
- [ ] Projects ordered by registration start date when location is declined
- [ ] Each project shows as a card with hero image, location, distance, and phase
- [ ] "Express Interest" button creates lightweight subscription
- [ ] Observer role can access limited dashboard with interested projects
- [ ] Email notifications sent X days before registration opens (configurable)
- [ ] Mobile-responsive design with Phoenix Admin Template (no sidebar)
- [ ] Privacy-compliant with ability to revoke location consent

## Technical Requirements

### Domain Layer

#### New Entities/Aggregates
```csharp
// BusinessIncubator.Domain/Aggregates/BusinessIncubator/ProjectInterest.cs
public class ProjectInterest : Entity
{
    public string UserId { get; private set; }
    public long ProjectId { get; private set; }
    public DateTime InterestedAt { get; private set; }
    public bool NotifyByEmail { get; private set; }
    public int NotifyDaysBefore { get; private set; }
    
    public static ProjectInterest Create(string userId, long projectId, bool notifyByEmail = true, int notifyDaysBefore = 7)
    {
        // Validation and creation logic
    }
    
    public void UpdateNotificationPreferences(bool notifyByEmail, int notifyDaysBefore)
    {
        // Update preferences
    }
}
```

#### Value Objects
```csharp
// Shared.Domain/ValueObjects/GeoCoordinate.cs
public class GeoCoordinate : ValueObject
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }
    
    public static Result<GeoCoordinate> Create(decimal latitude, decimal longitude)
    {
        // Validate: -90 <= latitude <= 90, -180 <= longitude <= 180
    }
    
    public double DistanceInKilometersTo(GeoCoordinate other)
    {
        // Haversine formula implementation
    }
}
```

#### Extend Project Entity
```csharp
public partial class Project
{
    private GeoCoordinate? _location;
    private string? _heroImageBlobId;
    private readonly List<ProjectInterest> _interests = [];
    
    public void SetLocation(decimal latitude, decimal longitude, IAuditContext auditContext)
    {
        var locationResult = GeoCoordinate.Create(latitude, longitude);
        if (locationResult.IsSuccess)
        {
            _location = locationResult.Value;
            SetUpdated(auditContext);
        }
    }
    
    public void SetHeroImage(string blobId, IAuditContext auditContext)
    {
        _heroImageBlobId = blobId;
        SetUpdated(auditContext);
    }
    
    public double? CalculateDistanceFrom(GeoCoordinate userLocation)
    {
        return _location?.DistanceInKilometersTo(userLocation);
    }
}
```

### Application Layer

#### Commands
```csharp
// ExpressInterestCommand.cs
public record ExpressInterestCommand(
    Guid ProjectExternalId,
    string UserId,
    bool NotifyByEmail = true,
    int NotifyDaysBefore = 7
) : IBaseRequest<ProjectInterestDto>;

// RequestLocationConsentCommand.cs
public record RequestLocationConsentCommand(
    string UserId,
    decimal Latitude,
    decimal Longitude
) : IBaseRequest;

// UpdateProjectLocationCommand.cs
public record UpdateProjectLocationCommand(
    Guid ProjectExternalId,
    decimal Latitude,
    decimal Longitude
) : IBaseRequest;
```

#### Queries
```csharp
// GetNearbyProjectsQuery.cs
public record GetNearbyProjectsQuery(
    decimal UserLatitude,
    decimal UserLongitude,
    double RadiusKm = 15,
    int PageSize = 20
) : IBaseRequest<FilteredQueryResult<PublicProjectDto>>;

// GetUpcomingProjectsQuery.cs
public record GetUpcomingProjectsQuery(
    int PageSize = 20,
    int PageNumber = 1
) : IBaseRequest<FilteredQueryResult<PublicProjectDto>>;

// GetUserInterestsQuery.cs
public record GetUserInterestsQuery(
    string UserId
) : IBaseRequest<List<ProjectInterestDto>>;
```

#### DTOs
```csharp
public class PublicProjectDto
{
    public Guid ExternalId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? HeroImageUrl { get; set; }
    public LocationDto Location { get; set; }
    public double? DistanceKm { get; set; }
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public string CurrentPhase { get; set; }
    public bool UserHasExpressedInterest { get; set; }
}
```

### Infrastructure Layer

#### Repository Methods
```csharp
public interface IBusinessIncubatorRepository
{
    Task<List<Project>> GetProjectsWithinRadiusAsync(
        decimal latitude, 
        decimal longitude, 
        double radiusKm,
        int limit,
        CancellationToken cancellationToken);
    
    Task<List<Project>> GetUpcomingProjectsByDateAsync(
        int skip,
        int take,
        CancellationToken cancellationToken);
    
    Task<ProjectInterest?> GetProjectInterestAsync(
        string userId,
        long projectId,
        CancellationToken cancellationToken);
}
```

#### SQL Implementation (using SQL Server geography)
```sql
-- GetProjectsWithinRadiusAsync implementation
DECLARE @userLocation geography = geography::Point(@latitude, @longitude, 4326);

SELECT TOP (@limit) p.*
FROM [businessincubators].[Projects] p
WHERE p.Latitude IS NOT NULL 
  AND p.Longitude IS NOT NULL
  AND geography::Point(p.Latitude, p.Longitude, 4326).STDistance(@userLocation) <= @radiusKm * 1000
ORDER BY geography::Point(p.Latitude, p.Longitude, 4326).STDistance(@userLocation);
```

### Web Layer

#### Controllers
```csharp
// Controllers/PublicProjectsController.cs (no auth required)
public class PublicProjectsController : BaseController
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Return public homepage view
        return View("PublicHomepage");
    }
    
    [AllowAnonymous]
    [HttpPost("nearby")]
    public async Task<IActionResult> GetNearbyProjects([FromBody] NearbyProjectsRequest request)
    {
        var query = new GetNearbyProjectsQuery(
            request.Latitude,
            request.Longitude,
            request.RadiusKm ?? 15);
        
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);
        return Json(result.Value);
    }
    
    [Authorize]
    [HttpPost("{projectId}/interest")]
    public async Task<IActionResult> ExpressInterest(Guid projectId)
    {
        var userId = User.GetUserId();
        var command = new ExpressInterestCommand(projectId, userId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
        
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = "Error al expresar interés" });
        }
        
        return Json(new { success = true });
    }
}
```

#### ViewModels
```csharp
public class PublicHomepageViewModel
{
    public List<PublicProjectDto> Projects { get; set; } = [];
    public bool LocationConsent { get; set; }
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
    public int DefaultRadiusKm { get; set; } = 15;
}

public class ObserverDashboardViewModel
{
    public List<ProjectInterestDto> InterestedProjects { get; set; } = [];
    public List<PublicProjectDto> NearbyProjects { get; set; } = [];
    public bool HasLocationConsent { get; set; }
}
```

#### JavaScript Implementation
```javascript
// wwwroot/js/public/geolocation-manager.js
window.GeolocationManager = (function() {
    let userLocation = null;
    let consentGiven = false;
    
    function requestLocation() {
        if (!navigator.geolocation) {
            showLocationNotSupported();
            return;
        }
        
        // Show consent modal
        showConsentModal().then(accepted => {
            if (accepted) {
                navigator.geolocation.getCurrentPosition(
                    position => {
                        userLocation = {
                            latitude: position.coords.latitude,
                            longitude: position.coords.longitude
                        };
                        consentGiven = true;
                        saveConsent();
                        loadNearbyProjects();
                    },
                    error => {
                        console.error('Location error:', error);
                        loadProjectsByDate();
                    },
                    { enableHighAccuracy: true, timeout: 10000 }
                );
            } else {
                loadProjectsByDate();
            }
        });
    }
    
    function loadNearbyProjects() {
        fetch('/PublicProjects/nearby', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                latitude: userLocation.latitude,
                longitude: userLocation.longitude,
                radiusKm: 15
            })
        })
        .then(response => response.json())
        .then(data => renderProjects(data.items, true));
    }
    
    function loadProjectsByDate() {
        fetch('/PublicProjects/upcoming')
            .then(response => response.json())
            .then(data => renderProjects(data.items, false));
    }
    
    function renderProjects(projects, showDistance) {
        const container = document.getElementById('projectGrid');
        container.innerHTML = projects.map(project => `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card project-card h-100">
                    ${project.heroImageUrl ? 
                        `<img src="${project.heroImageUrl}" class="card-img-top" alt="${project.name}">` : 
                        '<div class="card-img-placeholder"><i class="fas fa-project-diagram fa-3x"></i></div>'
                    }
                    <div class="card-body">
                        <h5 class="card-title">${project.name}</h5>
                        <p class="card-text">${project.description || ''}</p>
                        <div class="project-meta">
                            <span class="location">
                                <i class="fas fa-map-marker-alt"></i>
                                ${project.location.district}, ${project.location.province}
                            </span>
                            ${showDistance && project.distanceKm ? 
                                `<span class="distance">${project.distanceKm.toFixed(1)} km</span>` : ''
                            }
                        </div>
                        <div class="project-dates">
                            <small class="text-muted">
                                Inscripción: ${formatDate(project.registrationStartDate)}
                            </small>
                        </div>
                        <div class="project-phase">
                            <span class="badge bg-primary">${project.currentPhase}</span>
                        </div>
                    </div>
                    <div class="card-footer bg-transparent">
                        <button class="btn btn-primary btn-sm w-100 btn-interest" 
                                data-project="${project.externalId}"
                                ${project.userHasExpressedInterest ? 'disabled' : ''}>
                            ${project.userHasExpressedInterest ? 
                                '<i class="fas fa-check"></i> Ya te interesa' : 
                                '<i class="fas fa-star"></i> Me interesa'}
                        </button>
                    </div>
                </div>
            </div>
        `).join('');
        
        attachInterestHandlers();
    }
    
    function attachInterestHandlers() {
        document.querySelectorAll('.btn-interest:not([disabled])').forEach(btn => {
            btn.addEventListener('click', function() {
                const projectId = this.dataset.project;
                expressInterest(projectId);
            });
        });
    }
    
    function expressInterest(projectId) {
        // Check if user is authenticated
        if (!window.isAuthenticated) {
            // Store intent and redirect to registration
            sessionStorage.setItem('pendingInterest', projectId);
            window.location.href = `/Identity/Account/Register?returnUrl=/PublicProjects&role=Observer`;
            return;
        }
        
        fetch(`/PublicProjects/${projectId}/interest`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast('Interés registrado exitosamente', 'success');
                // Update button state
                const btn = document.querySelector(`[data-project="${projectId}"]`);
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-check"></i> Ya te interesa';
            } else {
                showToast('Error al registrar interés', 'danger');
            }
        });
    }
    
    return {
        init: function() {
            // Check for pending interest after registration
            const pendingInterest = sessionStorage.getItem('pendingInterest');
            if (pendingInterest && window.isAuthenticated) {
                expressInterest(pendingInterest);
                sessionStorage.removeItem('pendingInterest');
            }
            
            // Check for existing consent
            const hasConsent = localStorage.getItem('locationConsent');
            if (hasConsent === 'true') {
                requestLocation();
            } else {
                // Show location prompt banner
                document.getElementById('locationPrompt')?.classList.remove('d-none');
            }
        },
        requestLocation: requestLocation
    };
})();

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    if (document.getElementById('projectGrid')) {
        GeolocationManager.init();
    }
});
```

## Database Changes (SQL Server Database Project)

Since LinaSys uses a SQL Server Database Project (SSDT) and is not yet in production, we'll modify table definitions directly rather than using migrations.

### File Structure
All database changes will be made in the `Db/` folder following the SSDT structure:
- One file per database object
- Tables in `Db/{schema}/Tables/`
- Indexes in `Db/{schema}/Indexes/`
- Stored Procedures in `Db/{schema}/StoredProcedures/`
- Functions in `Db/{schema}/Functions/`

### Modified Tables

#### Db/businessincubators/Tables/Projects.sql
Add the following columns to the existing Projects table definition:

```sql
-- Add after [Status] column
-- Geolocation fields for REQ-011 with geohash support
[Latitude] DECIMAL(10, 8) NULL,
[Longitude] DECIMAL(11, 8) NULL,
[Geohash] VARCHAR(12) NULL, -- Full geohash for location
[GeohashPrefix5] AS LEFT([Geohash], 5) PERSISTED, -- ~5km precision for indexing
[GeohashPrefix6] AS LEFT([Geohash], 6) PERSISTED, -- ~1km precision for fine-tuning
[HeroImageBlobId] NVARCHAR(450) NULL,
[HasHeroImage] BIT NOT NULL CONSTRAINT [DF_Projects_HasHeroImage] DEFAULT (0),
[LocationUpdatedAt] DATETIME2 NULL,
[LocationUpdatedBy] NVARCHAR(256) NULL,
```

### New Tables

#### Db/businessincubators/Tables/ProjectInterests.sql
```sql
CREATE TABLE [businessincubators].[ProjectInterests] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [InterestedAt] DATETIME2 NOT NULL,
    [NotifyByEmail] BIT NOT NULL DEFAULT 1,
    [NotifyDaysBefore] INT NOT NULL DEFAULT 7,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(256) NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(256) NULL,
    CONSTRAINT [PK_ProjectInterests] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProjectInterests_Projects] FOREIGN KEY ([ProjectId]) 
        REFERENCES [businessincubators].[Projects] ([Id]),
    CONSTRAINT [FK_ProjectInterests_Users] FOREIGN KEY ([UserId]) 
        REFERENCES [auth].[AspNetUsers] ([Id]),
    CONSTRAINT [UQ_ProjectInterests_User_Project] UNIQUE ([UserId], [ProjectId])
);

```

#### Db/usermanagement/Tables/UserProfiles.sql
Add the following columns to the existing UserProfiles table definition:
```sql
-- Add location consent fields for REQ-011
[LocationConsentAt] DATETIME2 NULL,
[LocationConsentRevoked] BIT NOT NULL CONSTRAINT [DF_UserProfiles_LocationConsentRevoked] DEFAULT (0),
[LocationConsentMethod] NVARCHAR(50) NULL,
[HomeLatitude] DECIMAL(10, 8) NULL,
[HomeLongitude] DECIMAL(11, 8) NULL,
[HomeLocationUpdatedAt] DATETIME2 NULL,
[PreferredSearchRadiusKm] INT NULL CONSTRAINT [DF_UserProfiles_PreferredRadius] DEFAULT (15),
CONSTRAINT [CK_UserProfiles_PreferredRadius] CHECK ([PreferredSearchRadiusKm] BETWEEN 1 AND 100)
```

### New Indexes

#### Db/businessincubators/Indexes/IX_Projects_Geohash.sql
```sql
-- B-tree index on geohash for efficient proximity queries
CREATE NONCLUSTERED INDEX [IX_Projects_Geohash] 
ON [businessincubators].[Projects] ([GeohashPrefix5], [GeohashPrefix6])
INCLUDE ([Latitude], [Longitude], [Name], [ExternalId])
WHERE [Geohash] IS NOT NULL
```

#### Db/businessincubators/Indexes/IX_Projects_LatLon.sql
```sql
-- Bounding box support index for range queries
CREATE NONCLUSTERED INDEX [IX_Projects_LatLon] 
ON [businessincubators].[Projects] ([Latitude], [Longitude])
INCLUDE ([Name], [ExternalId], [GeohashPrefix5])
WHERE [Latitude] IS NOT NULL AND [Longitude] IS NOT NULL
```

#### Db/businessincubators/Indexes/IX_ProjectInterests_ProjectId.sql
```sql
CREATE NONCLUSTERED INDEX [IX_ProjectInterests_ProjectId] 
ON [businessincubators].[ProjectInterests] ([ProjectId])
INCLUDE ([UserId], [NotifyByEmail], [InterestedAt])
```

#### Db/businessincubators/Indexes/IX_ProjectInterests_UserId.sql
```sql
CREATE NONCLUSTERED INDEX [IX_ProjectInterests_UserId] 
ON [businessincubators].[ProjectInterests] ([UserId])
INCLUDE ([ProjectId], [NotifyByEmail], [InterestedAt])
```

### New Stored Procedures

#### Db/businessincubators/StoredProcedures/GetNearbyProjects.sql
```sql
CREATE PROCEDURE [businessincubators].[GetNearbyProjects]
    @UserLatitude DECIMAL(10, 8),
    @UserLongitude DECIMAL(11, 8),
    @RadiusKm FLOAT = 15,
    @MaxResults INT = 100,
    @UserId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Calculate bounding box for initial filtering
    DECLARE @LatDelta DECIMAL(10, 8) = @RadiusKm / 111.0; -- 1 degree latitude ≈ 111km
    DECLARE @LonDelta DECIMAL(11, 8) = @RadiusKm / (111.0 * COS(RADIANS(@UserLatitude)));
    
    DECLARE @MinLat DECIMAL(10, 8) = @UserLatitude - @LatDelta;
    DECLARE @MaxLat DECIMAL(10, 8) = @UserLatitude + @LatDelta;
    DECLARE @MinLon DECIMAL(11, 8) = @UserLongitude - @LonDelta;
    DECLARE @MaxLon DECIMAL(11, 8) = @UserLongitude + @LonDelta;
    
    -- Note: Actual distance calculation should be done in application layer using Haversine
    -- This returns candidates within bounding box for further filtering
    SELECT TOP (@MaxResults)
        p.Id,
        p.ExternalId,
        p.Name,
        p.Description,
        p.Latitude,
        p.Longitude,
        p.HeroImageBlobId,
        p.HasHeroImage,
        p.GeohashPrefix5,
        CASE WHEN pi.Id IS NOT NULL THEN 1 ELSE 0 END AS UserHasExpressedInterest
    FROM [businessincubators].[Projects] p
    LEFT JOIN [businessincubators].[ProjectInterests] pi
        ON pi.ProjectId = p.Id AND pi.UserId = @UserId
    WHERE p.IsDeleted = 0
        AND p.Status = 1 -- Active
        AND p.Latitude BETWEEN @MinLat AND @MaxLat
        AND p.Longitude BETWEEN @MinLon AND @MaxLon
    ORDER BY p.Latitude, p.Longitude; -- Rough ordering, precise distance in app layer
END
```

### Post-Deployment Scripts

#### Db/PostDeployment/011.SeedObserverRole.sql
```sql
-- Add Observer role if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM [auth].[AspNetRoles] WHERE [Name] = 'Observer')
BEGIN
    INSERT INTO [auth].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), 'Observer', 'OBSERVER', NEWID());
END
```

#### Db/PostDeployment/012.SeedProjectLocations.sql
```sql
-- Add sample location data to existing projects (Costa Rica coordinates)
UPDATE [businessincubators].[Projects]
SET [Latitude] = 
    CASE 
        WHEN Id % 5 = 0 THEN 9.9281  -- San José
        WHEN Id % 5 = 1 THEN 10.0163 -- Heredia
        WHEN Id % 5 = 2 THEN 9.9986  -- Cartago
        WHEN Id % 5 = 3 THEN 10.0729 -- Alajuela
        ELSE 9.9333                  -- Escazú
    END,
    [Longitude] = 
    CASE 
        WHEN Id % 5 = 0 THEN -84.0907 -- San José
        WHEN Id % 5 = 1 THEN -84.1197 -- Heredia
        WHEN Id % 5 = 2 THEN -83.9194 -- Cartago
        WHEN Id % 5 = 3 THEN -84.2227 -- Alajuela
        ELSE -84.1399                 -- Escazú
    END,
    [LocationUpdatedAt] = GETUTCDATE(),
    [LocationUpdatedBy] = 'SYSTEM_SEED'
WHERE [Latitude] IS NULL;
```

## UI/UX Requirements

### Public Homepage Layout
```razor
@{
    Layout = "_PublicLayout"; // New layout without sidebar
}
@model PublicHomepageViewModel

<div class="hero-section bg-gradient-primary text-white py-5">
    <div class="container">
        <div class="row align-items-center">
            <div class="col-lg-6">
                <h1 class="display-4 fw-bold mb-3">
                    Descubre Proyectos de Incubación Cerca de Ti
                </h1>
                <p class="lead mb-4">
                    Encuentra oportunidades de emprendimiento en tu área y mantente informado 
                    sobre próximas convocatorias.
                </p>
                <div id="locationPrompt" class="d-none">
                    <button class="btn btn-light btn-lg me-3" onclick="GeolocationManager.requestLocation()">
                        <i class="fas fa-location-arrow"></i> Compartir Mi Ubicación
                    </button>
                    <button class="btn btn-outline-light btn-lg" onclick="GeolocationManager.loadProjectsByDate()">
                        Ver Todos los Proyectos
                    </button>
                </div>
            </div>
            <div class="col-lg-6">
                <img src="/images/hero-illustration.svg" alt="Incubación" class="img-fluid">
            </div>
        </div>
    </div>
</div>

<div class="container py-5">
    <div class="row mb-4">
        <div class="col-md-6">
            <h2>Proyectos Disponibles</h2>
        </div>
        <div class="col-md-6 text-end">
            <div class="btn-group" role="group">
                <button type="button" class="btn btn-outline-primary active" id="btnGridView">
                    <i class="fas fa-th"></i> Cuadrícula
                </button>
                <button type="button" class="btn btn-outline-primary" id="btnListView">
                    <i class="fas fa-list"></i> Lista
                </button>
            </div>
        </div>
    </div>
    
    <div id="loadingIndicator" class="text-center py-5">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Cargando...</span>
        </div>
    </div>
    
    <div id="projectGrid" class="row">
        <!-- Projects will be rendered here by JavaScript -->
    </div>
    
    <div id="noProjects" class="text-center py-5 d-none">
        <i class="fas fa-inbox fa-3x text-muted mb-3"></i>
        <h4>No hay proyectos disponibles en este momento</h4>
        <p class="text-muted">Vuelve pronto para ver nuevas oportunidades</p>
    </div>
</div>

@section Scripts {
    <script src="~/js/public/geolocation-manager.js"></script>
    <script>
        window.isAuthenticated = @Json.Serialize(User.Identity.IsAuthenticated);
    </script>
}
```

### Observer Dashboard
```razor
@model ObserverDashboardViewModel
@{
    Layout = "_Layout"; // Standard layout with limited sidebar
    ViewData["Title"] = "Mis Proyectos de Interés";
}

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <h1 class="h3 mb-4">
                <i class="fas fa-star"></i> Mis Proyectos de Interés
            </h1>
        </div>
    </div>
    
    @if (Model.InterestedProjects.Any())
    {
        <div class="row mb-4">
            <div class="col-12">
                <h4>Proyectos que Sigues</h4>
                <div class="row">
                    @foreach (var project in Model.InterestedProjects)
                    {
                        <div class="col-md-6 col-lg-4 mb-3">
                            <div class="card">
                                <div class="card-body">
                                    <h5 class="card-title">@project.ProjectName</h5>
                                    <p class="card-text">
                                        <small class="text-muted">
                                            Interesado desde: @project.InterestedAt.ToString("dd/MM/yyyy")
                                        </small>
                                    </p>
                                    <div class="form-check form-switch">
                                        <input class="form-check-input" type="checkbox" 
                                               id="notify-@project.ProjectId" 
                                               @(project.NotifyByEmail ? "checked" : "")
                                               data-project="@project.ProjectId">
                                        <label class="form-check-label" for="notify-@project.ProjectId">
                                            Recibir notificaciones
                                        </label>
                                    </div>
                                    @if (project.NotifyByEmail)
                                    {
                                        <div class="mt-2">
                                            <label class="form-label small">Notificarme antes:</label>
                                            <select class="form-select form-select-sm" 
                                                    data-project="@project.ProjectId">
                                                <option value="1" @(project.NotifyDaysBefore == 1 ? "selected" : "")>
                                                    1 día antes
                                                </option>
                                                <option value="3" @(project.NotifyDaysBefore == 3 ? "selected" : "")>
                                                    3 días antes
                                                </option>
                                                <option value="7" @(project.NotifyDaysBefore == 7 ? "selected" : "")>
                                                    7 días antes
                                                </option>
                                                <option value="14" @(project.NotifyDaysBefore == 14 ? "selected" : "")>
                                                    14 días antes
                                                </option>
                                            </select>
                                        </div>
                                    }
                                </div>
                                <div class="card-footer bg-transparent">
                                    <a href="/PublicProjects/@project.ProjectExternalId" 
                                       class="btn btn-primary btn-sm w-100">
                                        Ver Detalles
                                    </a>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            </div>
        </div>
    }
    
    @if (Model.HasLocationConsent && Model.NearbyProjects.Any())
    {
        <div class="row">
            <div class="col-12">
                <h4>Proyectos Cercanos</h4>
                <div class="row">
                    @foreach (var project in Model.NearbyProjects.Where(p => !Model.InterestedProjects.Any(i => i.ProjectExternalId == p.ExternalId)))
                    {
                        <div class="col-md-6 col-lg-4 mb-3">
                            <div class="card">
                                <div class="card-body">
                                    <h5 class="card-title">@project.Name</h5>
                                    <p class="card-text">
                                        <i class="fas fa-map-marker-alt"></i> 
                                        @project.DistanceKm?.ToString("F1") km
                                    </p>
                                    <button class="btn btn-outline-primary btn-sm w-100 btn-express-interest"
                                            data-project="@project.ExternalId">
                                        <i class="fas fa-star"></i> Me Interesa
                                    </button>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            </div>
        </div>
    }
</div>

@section Scripts {
    <script src="~/js/observer/dashboard.js"></script>
}
```

### Mobile Responsive Considerations
- Card layout switches to single column on mobile
- Touch-friendly buttons (min 44x44px)
- Simplified navigation for Observer role
- Location prompt as bottom sheet on mobile
- Lazy loading for project images

## Dependencies
- [ ] Azure Blob Storage service (already exists)
- [ ] SQL Server geography types or spatial indexes
- [ ] Email notification infrastructure (exists)
- [ ] Phoenix Admin Template layouts (exists)
- [ ] Browser Geolocation API support

## Testing Requirements

### Unit Tests
- GeoCoordinate value object validation
- Haversine distance calculations
- Project interest creation and updates
- Location consent management
- Notification scheduling logic

### Integration Tests
- GetNearbyProjectsQuery with spatial queries
- ExpressInterestCommand with duplicate prevention
- Notification service integration
- Blob storage image retrieval

### E2E Scenarios
1. First-time visitor flow:
   - Visit homepage → Prompt for location → Accept → See nearby projects
   - Visit homepage → Prompt for location → Decline → See date-ordered projects

2. Interest expression flow:
   - Anonymous user → Express interest → Redirect to register → Complete registration → Interest saved

3. Observer dashboard flow:
   - Register as Observer → Express interests → View dashboard → Manage notifications

### Performance Tests
- Spatial query performance with 10,000+ projects
- Homepage load time < 2 seconds
- Image lazy loading effectiveness
- Caching strategy for location-based queries

## Security Considerations

### Authentication & Authorization
- Public homepage requires no authentication
- Express interest requires authentication (any role)
- Observer role has limited permissions:
  - Can view public projects
  - Can express interest
  - Can manage own notification preferences
  - Cannot access full participant features

### Privacy & Data Protection
- Explicit consent for location sharing
- Location data stored only with consent
- Ability to revoke consent and delete location data
- GDPR compliance for EU users
- Location precision limited to city/district level in UI

### Input Validation
- Validate latitude/longitude ranges
- Sanitize search radius (min: 1km, max: 100km)
- Rate limit interest expressions (10 per minute per user)
- Validate image uploads (type, size, content scanning)

## Documentation Updates
- [ ] Update CLAUDE.md with new Observer role
- [ ] Add geolocation patterns to web-patterns.md
- [ ] Document spatial query optimization in common-issues.md
- [ ] Add public homepage routes to architecture.md
- [ ] Create ADR for geolocation architecture decision

## Implementation Notes

### Configuration

Add to `appsettings.json`:
```json
{
  "GeolocationSettings": {
    "DefaultRadiusKm": 15,
    "MaxRadiusKm": 100,
    "MinRadiusKm": 1,
    "CacheDurationMinutes": 5,
    "GeohashPrecision": 5,  // 5 chars = ~5km precision
    "EnableLocationHistory": false
  }
}
```

### Geolocation Implementation Strategy
1. **Browser API First**: Use HTML5 Geolocation API for accuracy
2. **IP Fallback**: If browser location fails, use IP geolocation service
3. **Manual Override**: Allow users to manually enter location
4. **Caching**: Cache user location for session duration

### Why Geohash Strategy?
We chose the **Geohash-based approach** over SQL Server spatial features because:
1. **Azure SQL Cost Efficiency**: 80-90% reduction in DTU consumption
2. **Universal Compatibility**: Works on all Azure SQL tiers (Basic to Premium)
3. **Sufficient Accuracy**: ±1% distance error acceptable for 15km searches
4. **Better Scalability**: B-tree indexes outperform spatial indexes at scale
5. **Simpler Migration**: No dependency on specific SQL Server versions

### Performance Optimizations
1. **Geohash Indexes**: B-tree indexes on geohash prefixes
2. **Bounding Box Pre-filter**: Reduce candidates before distance calculation
3. **Query Caching**: Cache nearby projects for 5 minutes
4. **Image CDN**: Serve hero images through CDN
5. **Pagination**: Load projects in batches of 20
6. **Lazy Loading**: Load images only when visible

### Notification Implementation
```csharp
// Background service to check for upcoming registrations
public class ProjectNotificationService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var interests = await GetInterestsRequiringNotification();
            foreach (var interest in interests)
            {
                await SendNotificationEmail(interest);
                await MarkNotificationSent(interest);
            }
            
            // Run daily at 9 AM
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

## Definition of Done
- [ ] Database migrations created and tested
- [ ] Domain entities updated with location support
- [ ] CQRS commands/queries implemented
- [ ] Public homepage functional without authentication
- [ ] Geolocation working on Chrome, Firefox, Safari, Edge
- [ ] Observer role created and permissions configured
- [ ] Interest expression saves correctly
- [ ] Notifications sent at configured times
- [ ] Mobile responsive design verified
- [ ] Performance metrics met (< 2s load time)
- [ ] Security scan passed
- [ ] Unit tests achieving 80% coverage
- [ ] Integration tests passing
- [ ] E2E tests passing
- [ ] Documentation updated
- [ ] Code review completed
- [ ] Deployed to staging environment

## Follow-up Tasks
1. **Phase 2 - Advanced Features**:
   - Project recommendations based on interests
   - Social sharing capabilities
   - Project comparison tool
   - Advanced filtering (by category, duration, requirements)

2. **Phase 3 - Analytics**:
   - Conversion funnel tracking
   - Heat map of project interest by region
   - A/B testing for homepage layouts
   - Predictive analytics for project success

3. **Phase 4 - Mobile App**:
   - Native mobile app with push notifications
   - Offline support for viewed projects
   - Location-based notifications when near projects

## Risk Mitigation
- **Location Accuracy**: Some users may have inaccurate GPS, provide manual correction option
- **Browser Compatibility**: Older browsers lack geolocation, provide graceful degradation
- **Privacy Concerns**: Make location sharing clearly optional with benefits explained
- **Performance at Scale**: Implement proper caching and consider NoSQL for location queries if needed
- **Notification Delivery**: Use reliable email service with retry logic and delivery tracking