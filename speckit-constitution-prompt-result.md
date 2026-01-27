Generate a constitution.md file at `.specify/memory/constitution.md` that encodes the governing principles for the LinaSys business incubator platform. This constitution MUST be treated as binding law for
  all future /speckit.specify, /speckit.plan, /speckit.tasks, and /speckit.implement phases.

  ## MISSION & SCOPE

  This is an enterprise-grade ASP.NET Core 10 business incubator management system targeting Spanish-speaking markets. The platform manages incubators, projects, participants, knowledge structures,
  diagnostics, mentoring relationships, subscriptions, and notifications. The architecture is a modular monolith designed for eventual microservice extraction.

  All specifications must assume:
  - Spanish-language UI (all user-facing strings, validation messages, labels)
  - .NET 10.0 target framework with Aspire 13.0.2 orchestration
  - SQL Server database with SSDT project (no EF migrations)
  - Clean Architecture with mandatory layer separation

  ## ARCHITECTURAL MANDATES

  ### Layer Boundaries (ENFORCED AT BUILD TIME)
  1. **Domain Layer**: Pure business logic, aggregates, value objects, domain events. NO framework dependencies (no EF attributes, no ASP.NET types).
  2. **Application Layer**: Commands, queries, handlers, DTOs, integration events. References Domain only.
  3. **Infrastructure Layer**: EF Core DbContext, repository implementations, external services. References Domain and Application.
  4. **Web Layer**: Controllers, views, view models, filters. References all layers but NEVER bypasses Application for data access.

  Any specification that violates layer boundaries MUST be rejected.

  ### CQRS Pattern Requirements
  - Commands use `IBaseRequest` or `IBaseRequest<TResult>` (NOT `IRequest<Result<T>>`)
  - Handlers inherit from `BaseCommandHandler<TRequest>` or `BaseCommandHandler<TRequest, TResult>`
  - Return `Success()` or `Success(data)` directly, never nested Results
  - FluentValidation validators for all commands with user input
  - Queries are side-effect-free

  ### Domain-Driven Design Constraints
  - Aggregate roots control all modifications to child entities
  - Private backing fields for collections (`_items = new()`) with `.AsReadOnly()` access
  - Navigation properties marked `internal` for EF Core
  - Cross-aggregate references by ID only, never object references
  - Value objects are self-validating via factory methods
  - External-facing entities MUST have `ExternalId` (Guid); routes use ExternalId, NEVER internal IDs

  ### Integration Events (ADR-001)
  - Cross-domain communication via integration events is permitted
  - Events placed in originating domain's `Application/IntegrationEvents/`
  - Handlers implement `INotificationHandler<TEvent>`
  - Only event contracts and constants may be referenced across domains—no business logic sharing

  ## CODE QUALITY MANDATES

  ### Zero-Warnings Policy
  The build configuration enforces `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`. All StyleCop violations, nullable warnings, and compiler warnings MUST be resolved. Specifications that would introduce
   warnings are invalid.

  ### DateTime Handling (CRITICAL)
  NEVER use `DateTime.UtcNow` or `DateTime.Now` directly in Domain or Application layers.
  - Domain: Accept DateTime as method/constructor parameters
  - Application: Inject `ITimeProvider` and use `timeProvider.UtcNow`
  - Integration Events: DateTime as constructor parameter, never computed property

  ### Naming Conventions
  - Commands: `{Verb}{Entity}Command` (e.g., `CreateProjectCommand`)
  - Queries: `{Get|List}{Entity}Query` (e.g., `GetProjectByIdQuery`)
  - Handlers: `{CommandName}Handler`
  - DTOs: `{Entity}Dto`, `{Entity}DetailsDto`, `{Entity}ListItemDto`
  - ViewModels: `{Action}{Entity}ViewModel` (e.g., `CreateProjectViewModel`)
  - Repositories: `I{Aggregate}Repository` interface, `{Aggregate}Repository` implementation

  ### File Organization
  - One class per file, filename matches class name
  - JavaScript MUST reside in `/wwwroot/js/` (not Views folders)
  - SQL files use UTF-8 encoding without BOM
  - PostDeployment scripts in `/PostDeployment/` (outside Db/ folder)

  ## LOCALIZATION REQUIREMENTS

  ### Spanish-First UI
  ALL user-facing text must be in Spanish:
  - Validation messages: "El campo es requerido"
  - Toast notifications: "Operación exitosa", "Error al guardar"
  - Labels, buttons, headings, error pages
  - Email templates

  Code comments, variable names, and documentation remain English.

  ## DATABASE CONSTRAINTS

  ### SSDT/DACPAC Strategy
  - Schema managed via SQL Server Database Project (Db/LinaDb.sqlproj)
  - No Entity Framework migrations
  - Seed data via numbered PostDeployment scripts (000-014)
  - Scripts must be idempotent (safe for re-execution)

  ### SQL Syntax Rules
  - Index syntax: INCLUDE clause BEFORE WHERE clause
  - Computed columns in filtered indexes must be PERSISTED
  - No DBCC commands in post-deployment scripts

  ### Entity Framework Patterns
  - Use string-based Include for private collections: `.Include("_projectStages")`
  - Repository pattern with interfaces in Domain, implementations in Infrastructure
  - Unit of Work for SaveChangesAsync

  ## WEB LAYER PATTERNS

  ### Controller Patterns
  - Inherit from `BaseController` with `MediatorExecutor` injection
  - NEVER inject repositories into controllers
  - Use extension methods: `this.SetSuccessToast()`, `this.MapErrorsToModelStateAndSetErrorToast<T>()`
  - File uploads: Convert `IFormFile` to `Stream` at controller boundary before passing to commands

  ### UI Framework
  - Bootstrap 5 with Phoenix Admin Template
  - Toast notifications via `showToast(message, type)` JavaScript function
  - DataTables with server-side processing
  - jsTree for hierarchical data visualization
  - Form wizards with auto-save (30s interval, 5s debounce)

  ## TESTING REQUIREMENTS

  - xUnit for unit tests
  - Moq for mocking
  - FluentAssertions for assertions
  - Respawn for database cleanup in integration tests
  - InMemory EF provider for unit testing
  - Test projects follow `{Domain}.Tests` naming

  ## DEPENDENCY GOVERNANCE

  ### Approved Technologies
  - MediatR 14.x for CQRS
  - FluentValidation 12.x for validation
  - Riok.Mapperly 4.x for object mapping (source generators)
  - Entity Framework Core 10.x for ORM
  - MailKit/MimeKit for email
  - EPPlus for Excel generation
  - CsvHelper for CSV processing

  ### Forbidden Patterns
  - AutoMapper (use Mapperly instead)
  - Dapper for primary data access (use EF Core repositories)
  - Global exception handling that swallows errors
  - Service locator anti-pattern
  - Static classes for business logic

  ## SPECIFICATION VALIDATION RULES

  Before any /speckit.specify, /speckit.plan, or /speckit.tasks output is finalized, validate:

  1. Does the specification respect Clean Architecture layers?
  2. Are all DateTime values passed via parameters or ITimeProvider?
  3. Is all user-facing text in Spanish?
  4. Do new entities include ExternalId for external exposure?
  5. Are CQRS patterns followed (correct base classes, Result handling)?
  6. Would implementation introduce any compiler warnings?
  7. Are integration events correctly placed and minimal in scope?
  8. Does database work follow SSDT/PostDeployment conventions?

  If any validation fails, the specification MUST be revised before proceeding.

  ## ENFORCEMENT

  This constitution is binding. Claude Code agents executing /speckit workflows MUST:
  1. Load this constitution at the start of each workflow phase
  2. Validate all outputs against these principles
  3. Reject or revise any output that violates these mandates
  4. Document any ambiguity or edge cases encountered for future constitution updates

  Store at: `.specify/memory/constitution.md`