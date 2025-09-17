using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

public partial class Project : SoftDeletableEntity
{
    private readonly List<ProjectBlock> _projectBlocks = [];
    private readonly List<ProjectInvitation> _projectInvitations = [];
    private readonly List<BatchUserRegistration> _batchUserRegistrations = [];
    private readonly List<ProjectStage> _projectStages = [];

    public Project(string name, string? description, string key, long businessIncubatorId, IAuditContext auditContext)
    {
        ExternalId = Guid.NewGuid();
        Name = name;
        Description = description;
        Key = key;
        BusinessIncubatorId = businessIncubatorId;

        SetCreated(auditContext);
    }

    protected Project()
    {
    }

    public long BusinessIncubatorId { get; private set; }

    public Guid ExternalId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public string Key { get; private set; }

    public long? SourceFormId { get; private set; }

    public ProjectStatus Status { get; private set; }

    // Geolocation properties
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Geohash { get; private set; }
    public string? GeohashPrefix5 => Geohash?.Length >= 5 ? Geohash.Substring(0, 5) : null;
    public string? GeohashPrefix6 => Geohash?.Length >= 6 ? Geohash.Substring(0, 6) : null;
    public string? LocationName { get; private set; }
    public string? LocationAddress { get; private set; }
    public DateTime? LocationLastUpdated { get; private set; }

    // Hero image properties for public homepage (REQ-011)
    public string? HeroImageBlobId { get; private set; }
    public bool HasHeroImage { get; private set; }

    public IReadOnlyCollection<ProjectBlock> ProjectBlocks => _projectBlocks.AsReadOnly();

    public IReadOnlyCollection<ProjectInvitation> ProjectInvitations => _projectInvitations.AsReadOnly();

    public IReadOnlyCollection<BatchUserRegistration> BatchUserRegistrations => _batchUserRegistrations.AsReadOnly();

    public IReadOnlyCollection<ProjectStage> ProjectStages => _projectStages.AsReadOnly();

    // Navigation property for EF Core - not part of the aggregate
    internal virtual BusinessIncubator BusinessIncubator { get; private set; }

    // Navigation property for EF Core
    internal virtual ProjectKnowledgeStructure? ProjectKnowledgeStructure { get; private set; }

    public void ChangeStatus(ProjectStatus newStatus, IAuditContext auditableContext)
    {
        EnsureNotDeleted();

        if (Status == newStatus)
        {
            throw new InvalidOperationException("The status is already set to the specified value.");
        }

        Status = newStatus;
        SetUpdated(auditableContext);
    }

    public void Update(string name, string description, string key, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        Name = name.Trim();
        Description = description?.Trim();
        Key = key.Trim();

        SetUpdated(auditContext);
    }

    public void SetSourceForm(long? sourceFormId, IAuditContext auditContext)
    {
        EnsureNotDeleted();
        SourceFormId = sourceFormId;
        SetUpdated(auditContext);
    }

    public void ClearSourceForm(IAuditContext auditContext)
    {
        EnsureNotDeleted();
        SourceFormId = null;
        SetUpdated(auditContext);
    }

    public bool HasSourceForm() => SourceFormId.HasValue;

    public void UpdateLocation(
        decimal latitude,
        decimal longitude,
        string geohash,
        string? locationName,
        string? locationAddress,
        DateTime updatedAt,
        IAuditContext auditContext)
    {
        EnsureNotDeleted();

        // Validate latitude range (-90 to 90)
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentException("La latitud debe estar entre -90 y 90 grados.", nameof(latitude));
        }

        // Validate longitude range (-180 to 180)
        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentException("La longitud debe estar entre -180 y 180 grados.", nameof(longitude));
        }

        if (string.IsNullOrWhiteSpace(geohash))
        {
            throw new ArgumentException("El geohash es requerido.", nameof(geohash));
        }

        Latitude = latitude;
        Longitude = longitude;
        Geohash = geohash;
        LocationName = locationName?.Trim();
        LocationAddress = locationAddress?.Trim();
        LocationLastUpdated = updatedAt;

        SetUpdated(auditContext);
    }

    public void ClearLocation(IAuditContext auditContext)
    {
        EnsureNotDeleted();

        Latitude = null;
        Longitude = null;
        Geohash = null;
        LocationName = null;
        LocationAddress = null;
        LocationLastUpdated = null;

        SetUpdated(auditContext);
    }

    public bool HasLocation() => Latitude.HasValue && Longitude.HasValue;

    public void SetHeroImage(string heroImageBlobId, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(heroImageBlobId))
        {
            throw new ArgumentException("Hero image blob ID cannot be null or empty.", nameof(heroImageBlobId));
        }

        HeroImageBlobId = heroImageBlobId;
        HasHeroImage = true;

        SetUpdated(auditContext);
    }

    public void RemoveHeroImage(IAuditContext auditContext)
    {
        EnsureNotDeleted();

        HeroImageBlobId = null;
        HasHeroImage = false;

        SetUpdated(auditContext);
    }

    public ProjectBlock AddBlock(string name, long? sourceBlockId, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        if (sourceBlockId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceBlockId), "Source block ID must be a positive integer or null.");
        }

        if (sourceBlockId is not null && _projectBlocks.Any(a => a.SourceBlockId == sourceBlockId))
        {
            throw new InvalidOperationException("A block with the same source block ID already exists in this project.");
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new ArgumentException("Block name must not be empty and not exceed 100 characters.", nameof(name));
        }

        if (_projectBlocks.Any(b => b.Name == name))
        {
            throw new InvalidOperationException("A block with the same name already exists in this project.");
        }

        var block = new ProjectBlock(Id, sourceBlockId, name, false);

        _projectBlocks.Add(block);
        SetUpdated(auditContext);

        return block;
    }

    public void RemoveBlock(ProjectBlock block)
    {
        EnsureNotDeleted();
        if (!_projectBlocks.Remove(block))
        {
            throw new InvalidOperationException("The specified block does not exist in this project.");
        }
    }

    public void ClearAllBlocks(IAuditContext auditContext)
    {
        EnsureNotDeleted();
        _projectBlocks.Clear();
        SetUpdated(auditContext);
    }

    public void ClearKnowledgeStructure(IAuditContext auditContext)
    {
        EnsureNotDeleted();

        // Clear the knowledge structure reference
        // The actual clearing of modules should be handled by the ProjectKnowledgeStructure aggregate
        ProjectKnowledgeStructure = null;
        SetUpdated(auditContext);
    }

    public void ResetProjectContent(IAuditContext auditContext)
    {
        EnsureNotDeleted();
        _projectBlocks.Clear();

        // Clear the knowledge structure reference
        // The actual clearing of modules should be handled by the ProjectKnowledgeStructure aggregate
        ProjectKnowledgeStructure = null;
        SetUpdated(auditContext);
    }

    public ProjectKnowledgeStructure SetKnowledgeStructure(
        long? sourceKnowledgeStructureId,
        string name,
        bool isNameCustomized,
        string? description,
        bool isDescriptionCustomized,
        IAuditContext auditContext)
    {
        EnsureNotDeleted();

        if (ProjectKnowledgeStructure is not null)
        {
            throw new InvalidOperationException("A different knowledge structure is already set for this project.");
        }

        if (string.IsNullOrEmpty(name) || name.Length > 100)
        {
            throw new ArgumentException("Name must not be empty and not exceed 100 characters.", nameof(name));
        }

        if (!string.IsNullOrEmpty(description) && description.Length > 500)
        {
            throw new ArgumentException("Description must not exceed 500 characters.", nameof(description));
        }

        if (isDescriptionCustomized && description is null)
        {
            throw new ArgumentException("Description cannot be null when isDescriptionCustomized is true.", nameof(description));
        }

        if (sourceKnowledgeStructureId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceKnowledgeStructureId), "Source knowledge structure ID must be a positive integer or null.");
        }

        var knowledgeStructure = new ProjectKnowledgeStructure(
            sourceKnowledgeStructureId,
            Id,
            name,
            isNameCustomized,
            description,
            isDescriptionCustomized);

        ProjectKnowledgeStructure = knowledgeStructure;
        SetUpdated(auditContext);

        return ProjectKnowledgeStructure;
    }

    /// <summary>
    /// Creates a new project invitation.
    /// </summary>
    /// <param name="email">The email address of the user to invite.</param>
    /// <param name="fullName">The full name of the user to invite.</param>
    /// <param name="identificationNumber">The identification number of the user to invite.</param>
    /// <param name="role">The role ID to assign to the user in the project.</param>
    /// <param name="expirationDays">The number of days until the invitation expires.</param>
    /// <param name="auditContext">The audit context.</param>
    /// <returns>The created project invitation.</returns>
    public ProjectInvitation CreateInvitation(string email, string fullName, string identificationNumber, string? role, int expirationDays, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
        }

        if (string.IsNullOrWhiteSpace(identificationNumber))
        {
            throw new ArgumentException("Identification number cannot be empty.", nameof(identificationNumber));
        }

        if (expirationDays <= 0)
        {
            throw new ArgumentException("Expiration days must be positive.", nameof(expirationDays));
        }

        // Check if there's already a pending invitation for this email
        if (_projectInvitations.Any(i => i.Email == email && i.Status == ProjectInvitationStatus.Pending))
        {
            throw new InvalidOperationException("There is already a pending invitation for this email address.");
        }

        var invitation = ProjectInvitation.Create(
            Id,
            email,
            fullName,
            identificationNumber,
            role,
            expirationDays,
            auditContext);

        _projectInvitations.Add(invitation);
        SetUpdated(auditContext);

        return invitation;
    }

    /// <summary>
    /// Creates a new batch user registration.
    /// </summary>
    /// <param name="fileName">The name of the uploaded CSV file.</param>
    /// <param name="totalUsers">The total number of users to register.</param>
    /// <param name="auditContext">The audit context.</param>
    /// <returns>The created batch user registration.</returns>
    public BatchUserRegistration CreateBatchUserRegistration(string fileName, int totalUsers, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        }

        if (totalUsers <= 0)
        {
            throw new ArgumentException("Total users must be positive.", nameof(totalUsers));
        }

        var batchRegistration = BatchUserRegistration.Create(
            Id,
            fileName,
            totalUsers,
            auditContext);

        _batchUserRegistrations.Add(batchRegistration);
        SetUpdated(auditContext);

        return batchRegistration;
    }

    /// <summary>
    /// Gets all pending invitations for this project.
    /// </summary>
    /// <param name="currentTime">The current timestamp to check expiration against.</param>
    /// <returns>A collection of pending invitations.</returns>
    public IEnumerable<ProjectInvitation> GetPendingInvitations(DateTime currentTime)
    {
        return _projectInvitations.Where(i => i.Status == ProjectInvitationStatus.Pending && !i.IsExpired(currentTime));
    }

    /// <summary>
    /// Gets all active batch registrations for this project.
    /// </summary>
    /// <returns>A collection of active batch registrations.</returns>
    public IEnumerable<BatchUserRegistration> GetActiveBatchRegistrations()
    {
        return _batchUserRegistrations.Where(b =>
            b.Status == BatchUserRegistrationStatus.Pending ||
            b.Status == BatchUserRegistrationStatus.Processing);
    }

    /// <summary>
    /// Checks if a block with the given source ID exists in this project.
    /// </summary>
    /// <param name="sourceBlockId">The source block ID to check.</param>
    /// <returns>True if exists, false otherwise.</returns>
    public bool HasBlockWithSourceId(long sourceBlockId)
    {
        return _projectBlocks.Any(b => b.SourceBlockId == sourceBlockId);
    }

    /// <summary>
    /// Checks if a block with the given name exists in this project.
    /// </summary>
    /// <param name="blockName">The block name to check.</param>
    /// <returns>True if exists, false otherwise.</returns>
    public bool HasBlockWithName(string blockName)
    {
        return _projectBlocks.Any(b => b.Name == blockName);
    }

    /// <summary>
    /// Finds a project block by its source ID.
    /// </summary>
    /// <param name="sourceBlockId">The source block ID.</param>
    /// <returns>The project block if found, null otherwise.</returns>
    public ProjectBlock? FindBlockBySourceId(long? sourceBlockId)
    {
        return sourceBlockId.HasValue
            ? _projectBlocks.FirstOrDefault(b => b.SourceBlockId == sourceBlockId.Value)
            : null;
    }

    /// <summary>
    /// Finds a project topic by its source ID.
    /// </summary>
    /// <param name="sourceTopicId">The source topic ID.</param>
    /// <returns>The project topic if found, null otherwise.</returns>
    public ProjectTopic? FindTopicBySourceId(long? sourceTopicId)
    {
        if (sourceTopicId is null || ProjectKnowledgeStructure is null)
        {
            return null;
        }

        return ProjectKnowledgeStructure.FindTopicBySourceId(sourceTopicId.Value);
    }

    /// <summary>
    /// Gets the current knowledge structure version.
    /// </summary>
    /// <returns>The current version or 1 if no knowledge structure is set.</returns>
    public int GetCurrentKnowledgeStructureVersion()
    {
        return ProjectKnowledgeStructure?.CurrentVersion ?? 1;
    }

    /// <summary>
    /// Checks if the project has a knowledge structure.
    /// </summary>
    /// <returns>True if the project has a knowledge structure, false otherwise.</returns>
    public bool HasKnowledgeStructure()
    {
        return ProjectKnowledgeStructure is not null;
    }

    /// <summary>
    /// Gets the knowledge structure if available.
    /// </summary>
    /// <returns>The project knowledge structure or null.</returns>
    public ProjectKnowledgeStructure? GetKnowledgeStructure()
    {
        return ProjectKnowledgeStructure;
    }

    #region Stage Management

    /// <summary>
    /// Adds a new stage to the project.
    /// </summary>
    /// <param name="type">The stage type.</param>
    /// <param name="title">The stage title.</param>
    /// <param name="description">The stage description (optional).</param>
    /// <param name="startDate">The stage start date.</param>
    /// <param name="endDate">The stage end date.</param>
    /// <param name="auditContext">The audit context.</param>
    /// <returns>The created project stage.</returns>
    public ProjectStage AddStage(
        ProjectStageType type,
        string title,
        string? description,
        DateTime startDate,
        DateTime endDate,
        IAuditContext auditContext)
    {
        EnsureNotDeleted();

        // Check if a stage of this type already exists
        if (_projectStages.Any(s => s.Type == type))
        {
            throw new InvalidOperationException($"A stage of type {type} already exists for this project.");
        }

        // Check for overlapping date ranges with existing stages
        if (_projectStages.Any(s =>
            (startDate >= s.StartDate && startDate <= s.EndDate) ||
            (endDate >= s.StartDate && endDate <= s.EndDate) ||
            (startDate <= s.StartDate && endDate >= s.EndDate)))
        {
            throw new InvalidOperationException("The stage dates overlap with an existing stage.");
        }

        var stage = ProjectStage.Create(
            Id,
            type,
            title,
            description,
            startDate,
            endDate,
            auditContext);

        _projectStages.Add(stage);
        SetUpdated(auditContext);

        return stage;
    }

    /// <summary>
    /// Removes a stage from the project.
    /// </summary>
    /// <param name="type">The stage type to remove.</param>
    /// <param name="auditContext">The audit context.</param>
    public void RemoveStage(ProjectStageType type, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var stage = _projectStages.FirstOrDefault(s => s.Type == type);
        if (stage is null)
        {
            throw new InvalidOperationException($"No stage of type {type} exists for this project.");
        }

        _projectStages.Remove(stage);
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Gets the current active stage based on the provided date.
    /// </summary>
    /// <param name="currentDate">The date to check against.</param>
    /// <returns>The current stage if found, null otherwise.</returns>
    public ProjectStage? GetCurrentStage(DateTime currentDate)
    {
        return _projectStages
            .Where(s => s.IsActive && s.IsWithinPeriod(currentDate))
            .OrderBy(s => s.Type)
            .FirstOrDefault();
    }

    /// <summary>
    /// Checks if the project is in a specific stage at the given date.
    /// </summary>
    /// <param name="type">The stage type to check.</param>
    /// <param name="currentDate">The date to check against.</param>
    /// <returns>True if the project is in the specified stage, false otherwise.</returns>
    public bool IsInStage(ProjectStageType type, DateTime currentDate)
    {
        var stage = _projectStages.FirstOrDefault(s => s.Type == type);
        return stage is not null && stage.IsCurrent(currentDate);
    }

    /// <summary>
    /// Gets a specific stage by type.
    /// </summary>
    /// <param name="type">The stage type to retrieve.</param>
    /// <returns>The stage if found, null otherwise.</returns>
    public ProjectStage? GetStage(ProjectStageType type)
    {
        return _projectStages.FirstOrDefault(s => s.Type == type);
    }

    /// <summary>
    /// Activates a specific stage.
    /// </summary>
    /// <param name="type">The stage type to activate.</param>
    /// <param name="auditContext">The audit context.</param>
    public void ActivateStage(ProjectStageType type, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var stage = _projectStages.FirstOrDefault(s => s.Type == type);
        if (stage is null)
        {
            throw new InvalidOperationException($"No stage of type {type} exists for this project.");
        }

        stage.Activate(auditContext);
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Deactivates a specific stage.
    /// </summary>
    /// <param name="type">The stage type to deactivate.</param>
    /// <param name="auditContext">The audit context.</param>
    public void DeactivateStage(ProjectStageType type, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var stage = _projectStages.FirstOrDefault(s => s.Type == type);
        if (stage is null)
        {
            throw new InvalidOperationException($"No stage of type {type} exists for this project.");
        }

        stage.Deactivate(auditContext);
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Updates a stage's dates.
    /// </summary>
    /// <param name="type">The stage type to update.</param>
    /// <param name="startDate">The new start date.</param>
    /// <param name="endDate">The new end date.</param>
    /// <param name="auditContext">The audit context.</param>
    public void UpdateStageDates(ProjectStageType type, DateTime startDate, DateTime endDate, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var stage = _projectStages.FirstOrDefault(s => s.Type == type);
        if (stage is null)
        {
            throw new InvalidOperationException($"No stage of type {type} exists for this project.");
        }

        // Check for overlapping with other stages (excluding the current one)
        if (_projectStages.Any(s => s.Type != type &&
            ((startDate >= s.StartDate && startDate <= s.EndDate) ||
            (endDate >= s.StartDate && endDate <= s.EndDate) ||
            (startDate <= s.StartDate && endDate >= s.EndDate))))
        {
            throw new InvalidOperationException("The new dates overlap with another stage.");
        }

        stage.UpdateDates(startDate, endDate, auditContext);
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Updates a stage's details.
    /// </summary>
    /// <param name="type">The stage type to update.</param>
    /// <param name="title">The new title (optional).</param>
    /// <param name="description">The new description (optional).</param>
    /// <param name="auditContext">The audit context.</param>
    public void UpdateStageDetails(ProjectStageType type, string? title, string? description, IAuditContext auditContext)
    {
        EnsureNotDeleted();

        var stage = _projectStages.FirstOrDefault(s => s.Type == type);
        if (stage is null)
        {
            throw new InvalidOperationException($"No stage of type {type} exists for this project.");
        }

        stage.UpdateDetails(title, description, auditContext);
        SetUpdated(auditContext);
    }

    /// <summary>
    /// Gets all stages ordered by type.
    /// </summary>
    /// <returns>A collection of stages ordered by their type.</returns>
    public IEnumerable<ProjectStage> GetStagesOrdered()
    {
        return _projectStages.OrderBy(s => s.Type);
    }

    /// <summary>
    /// Checks if the project has any stages defined.
    /// </summary>
    /// <returns>True if the project has stages, false otherwise.</returns>
    public bool HasStages()
    {
        return _projectStages.Any();
    }

    #endregion
}
