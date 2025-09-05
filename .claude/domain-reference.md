# LinaSys Domain Reference

## Key Domain Entities

### Project Aggregate Root
- Central entity for business incubator projects
- Contains blocks, knowledge structure, invitations, and batch registrations
- Methods enforce business rules and invariants

### Project Relationships
```
Project
├── ProjectBlocks
├── ProjectKnowledgeStructure
│   └── ProjectModules
│       └── ProjectTopics
│           └── ProjectQuestions
│               └── ProjectAnswerOptions
├── ProjectInvitations
└── BatchUserRegistrations
```

## Domain Methods

### Project Methods
```csharp
// Block management
AddBlock(string name, string description)
- Creates ProjectBlock with duplicate validation
- Returns Result with error if name already exists

// Invitation management  
CreateInvitation(string email, string fullName, string identificationNumber, Guid roleId)
- Generates unique invitation token
- Sets expiration date (30 days)
- Validates email format

// Knowledge structure
GetKnowledgeStructure()
- Returns the project's knowledge structure
- Includes modules, topics, questions
```

### ProjectTopic Methods
```csharp
AddProjectQuestion(string statement, QuestionType type, List<AnswerOption> options)
- Creates question with answer options
- Validates FODA/ODSR types
- Ensures required number of options
```

### ProjectQuestion Methods
```csharp
AddProjectAnswerOption(string statement, string code, char fodaOdsrType)
- Maps single character to enum (F→Fortaleza, O→Oportunidad, etc.)
- Validates type matches question type
- Prevents duplicate codes
```

### ProjectInvitation Methods
```csharp
Accept() - Changes status to Accepted
Decline() - Changes status to Declined  
Revoke() - Changes status to Revoked
IsExpired() - Checks if past expiration date
MarkAsExpired() - Sets status to Expired
```

## Repository Methods

### IBusinessIncubatorRepository

#### Project Queries
```csharp
GetByExternalIdAsync(Guid externalId)
- Basic project without includes

GetWithProjectBlocksByExternalId(Guid externalId)
- Includes ProjectBlocks collection

GetWithProjectAndKnowledgeStructureByExternalId(Guid externalId)
- Includes full knowledge structure hierarchy

GetProjectWithInvitationsByExternalIdAsync(Guid externalId)
- Includes ProjectInvitations collection

GetAllProjectsWithInvitationsAsync()
- For invitation token lookup across all projects
```

#### Batch Registration Queries
```csharp
GetBatchUserRegistrationWithDetailsByIdAsync(Guid id)
- Includes registration details and project info

GetProjectWithBatchRegistrationsAsync(Guid projectExternalId)
- Gets project with all batch registrations
```

#### Persistence
```csharp
AddAsync(T entity)
- Adds new entity to context

Update(T entity)
- Updates existing entity

SaveChangesAsync()
- Persists all changes to database
```

### IUserRepository (Auth Domain)
```csharp
GetByEmailAsync(string email)
- Find user by email

GetUsersByEmailsAsync(List<string> emails)
- Bulk user lookup for batch operations

ExistsByEmailAsync(string email)
- Check if email already registered
```

### IUserProfileRepository
```csharp
GetByUserIdAsync(Guid userId)
- Get profile for specific user

AddAsync(UserProfile profile)
- Create new user profile
```

## Domain Services

### Business Rules
- Email uniqueness validation
- Invitation expiration (30 days)
- FODA requires exactly 4 options (F,O,D,A)
- ODSR requires exactly 4 options (O,D,S,R)
- Project blocks must have unique names

### Domain Events
- UserInvitedToProject
- InvitationAccepted
- BatchRegistrationCompleted
- ProjectQuestionAnswered

## Value Objects

### EmailAddress
- Validates email format
- Normalizes to lowercase
- Immutable once created

### InvitationToken
- Cryptographically secure random token
- URL-safe characters
- 32 character length

### ProjectCode
- Unique project identifier
- Format validation
- Used in URLs and references

## UserManagement Domain (NEW)

### UserProfile Aggregate Root
- Manages user profile information separate from authentication
- Contains business data (names, location, avatar)
- No direct references to other domains

### UserProfile Methods
```csharp
// Profile management
UpdateProfile(string firstName, string lastName, IAuditContext audit)
- Updates basic profile information
- Validates required fields
- Updates audit fields

UpdateLocation(Location location, IAuditContext audit)
- Updates user location (value object)
- Validates country-specific requirements

UpdateAvatar(string avatarUrl, IAuditContext audit)
- Updates avatar URL
- Should validate URL format

Deactivate(IAuditContext audit)
- Sets IsActive to false
- Updates audit fields

Reactivate(IAuditContext audit)
- Sets IsActive to true
- Updates audit fields
```

### Value Objects

#### Location
- Country, Province, Canton, District, FullAddress
- Validates Costa Rica-specific requirements
- Immutable once created

### IUserManagementRepository
```csharp
GetByUserIdAsync(string userId)
- Get profile by Identity user ID

GetByIdAsync(long id)
- Get profile by internal ID

ListProfilesAsync(UserProfileFilter filter)
- List profiles with filtering/paging

AddAsync(UserProfile profile)
- Create new profile

Update(UserProfile profile)
- Update existing profile
```

## Aggregate Boundaries
- Project is the main aggregate root (BusinessIncubator domain)
- UserProfile is aggregate root (UserManagement domain)
- ProjectInvitation lifecycle managed by Project
- BatchUserRegistration tracks bulk operations
- Cross-aggregate references use IDs only
- **No direct references between domains**

## Persistence Patterns
- Repositories return domain entities
- Entity Framework configurations in Infrastructure
- Audit fields (CreatedAt, UpdatedAt, etc.) automatically managed
- Soft deletes where applicable (IsDeleted flag)
- **Each domain has its own DbContext**
- **No foreign keys between domain schemas**