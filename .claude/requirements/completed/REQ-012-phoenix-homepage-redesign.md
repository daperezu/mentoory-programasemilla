# REQ-012: Phoenix Admin Template Homepage Redesign

## Overview
Redesign the public Home page and Project Detail flow using Phoenix Admin Template v1.22.0 components while maintaining optional geolocation support from REQ-011.

## Status
- **Created**: 2025-01-15
- **Status**: Active
- **Priority**: High
- **Complexity**: Medium

## Business Requirements
1. Modernize public homepage with Phoenix Admin Template design
2. Default to time-based project discovery (no location required)
3. Maintain geolocation as optional enhancement
4. Create professional project detail pages
5. Improve user engagement with modern UX

## Technical Requirements

### Dual Discovery Modes
1. **Default Mode** (Time-based)
   - No location permission required
   - Show top 10 projects by soonest start date
   - Secondary sort by project name
   - Display "Ordenado por: Fecha de inicio"

2. **Enhanced Mode** (Location-aware)
   - Activated when user shares location
   - Sort by proximity, then by start date
   - Display distance badges on cards
   - Display "Ordenado por: Cercanía"

### Phoenix Components to Use
- Landing page hero with gradient (`pages/landing/default.html`)
- Event detail layout (`apps/events/event-detail.html`)
- Card components with hover effects
- Timeline/stepper for "How it works"
- Badge components for metadata
- Responsive grid system

## Implementation Phases

### Phase 1: Backend Enhancements
- Create `GetLatestProjectsQuery` for time-based discovery
- Update DTOs to include ProjectStage dates
- Add seed data for 15-20 projects with stages
- Maintain existing geolocation functionality

### Phase 2: Phoenix Layout Integration
- Update `_PublicLayout.cshtml` with Phoenix navbar
- Add theme toggle (dark/light mode)
- Create Phoenix-specific CSS overrides
- Initialize Phoenix JavaScript components

### Phase 3: Homepage Redesign
- Hero section with dual CTAs
- Value proposition cards (6 features)
- Project discovery section (dual mode)
- "How it works" timeline
- Testimonials/social proof

### Phase 4: Project Details Page
- Create details view based on event-detail
- Hero image with overlay
- Metadata chips and badges
- Stage/schedule information
- Interest tracking integration

### Phase 5: Testing & Polish
- Test both discovery modes
- Responsive design validation
- Performance optimization
- Spanish translation verification

## Acceptance Criteria
- [ ] Homepage loads without location permission
- [ ] Shows top 10 projects by start date (default)
- [ ] Location sharing enhances with proximity sorting
- [ ] Phoenix components used throughout
- [ ] Details page mirrors event-detail template
- [ ] All user-facing text in Spanish
- [ ] Clean build (0 errors, 0 warnings)
- [ ] Database changes deployable via PowerShell script

## Technical Decisions

### Sorting Strategy
```csharp
// Default (no location)
projects.OrderBy(p => p.StartDate).ThenBy(p => p.Name)

// Enhanced (with location)
projects.OrderBy(p => p.DistanceKm).ThenBy(p => p.StartDate)
```

### Component Architecture
- Maintain existing JavaScript for geolocation
- Add Phoenix initialization layer
- Use partial views for reusable components
- Keep AJAX endpoints for location-based search

## Files to Create/Modify

### New Files
- `BusinessIncubator.Application/Public/Queries/GetLatestProjectsQuery.cs`
- `BusinessIncubator.Application/Public/Queries/GetLatestProjectsQueryHandler.cs`
- `BusinessIncubator.Application/Public/Queries/GetProjectDetailsQuery.cs`
- `BusinessIncubator.Application/Public/Queries/GetProjectDetailsQueryHandler.cs`
- `Web/Areas/Public/Views/Projects/Details.cshtml`
- `Web/Areas/Public/Views/Projects/_ProjectCard.cshtml`
- `Web/wwwroot/css/public-phoenix.css`
- `Web/wwwroot/js/phoenix-init.js`
- `Db/PostDeployment/011.SeedPublicProjects.sql`

### Modified Files
- `Web/Areas/Public/Controllers/ProjectsController.cs`
- `Web/Areas/Public/Views/Projects/Index.cshtml`
- `Web/Views/Shared/_PublicLayout.cshtml`
- `Web/wwwroot/js/public-projects.js`
- `BusinessIncubator.Application/Public/Queries/NearbyProjectsDto.cs`

## Dependencies
- Phoenix Admin Template v1.22.0 (already in project)
- Existing geolocation infrastructure (REQ-011)
- ProjectStages table and domain model
- Bootstrap 5.x

## Risk Mitigation
- Keep existing functionality intact during redesign
- Test both modes thoroughly before deployment
- Ensure backward compatibility with existing data
- Maintain clean architecture principles

## Notes
- Phoenix assets located in `/Assets/phoenix-v1.22.0/`
- Maintain Spanish UI throughout
- Follow LinaSys coding standards
- Zero warnings policy enforced