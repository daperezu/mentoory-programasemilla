using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Repositories;

/// <summary>
/// Repository interface for managing Business Incubator entities.
/// </summary>
public interface IBusinessIncubatorRepository : IRepository<Aggregates.BusinessIncubator.BusinessIncubator>
{
    /// <summary>
    /// Adds a new Business Incubator to the repository.
    /// </summary>
    /// <param name="businessIncubator">The Business Incubator entity to add.</param>
    /// <returns>The added Business Incubator entity.</returns>
    Aggregates.BusinessIncubator.BusinessIncubator Add(Aggregates.BusinessIncubator.BusinessIncubator businessIncubator);

    /// <summary>
    /// Adds a new project to the repository.
    /// </summary>
    /// <param name="project">The project to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddProjectAsync(Aggregates.BusinessIncubator.Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new review.
    /// </summary>
    /// <param name="review">The review to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddReviewAsync(Aggregates.BusinessIncubator.ProjectFormReview review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a Business Incubator exists by its key.
    /// </summary>
    /// <param name="key">The key of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a Business Incubator with the specified key exists, otherwise false.</returns>
    Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a Business Incubator exists by its key, excluding a specific entity.
    /// </summary>
    /// <param name="externalId">The external ID of the Business Incubator to exclude.</param>
    /// <param name="key">The key of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a Business Incubator with the specified key exists, otherwise false.</returns>
    Task<bool> ExistsByKeyNotItselfAsync(Guid externalId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a Business Incubator exists by its name.
    /// </summary>
    /// <param name="name">The name of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a Business Incubator with the specified name exists, otherwise false.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a Business Incubator exists by its name, excluding a specific entity.
    /// </summary>
    /// <param name="externalId">The external ID of the Business Incubator to exclude.</param>
    /// <param name="name">The name of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a Business Incubator with the specified name exists, otherwise false.</returns>
    Task<bool> ExistsByNameNotItselfAsync(Guid externalId, string name, CancellationToken cancellationToken = default);

    Task<bool> ExistsProjectByExternalIdAsync(Guid businessIncubatorExternalId, Guid projectExternalId, CancellationToken cancellationToken = default);

    Task<List<Aggregates.BusinessIncubator.BusinessIncubator>> GetAllIncubators(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all projects with their invitations for invitation token lookup.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all projects with their invitations.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetAllProjectsWithInvitationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a batch registration by its external ID.
    /// </summary>
    /// <param name="batchExternalId">The external ID of the batch registration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The BatchUserRegistration entity if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BatchUserRegistration?> GetBatchRegistrationByExternalIdAsync(Guid batchExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a business incubator by its external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <returns>The business incubator if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetBusinessIncubatorByExternalIdAsync(Guid externalId);

    /// <summary>
    /// Retrieves a Business Incubator by its external ID.
    /// </summary>
    /// <param name="externalId">The external ID of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Business Incubator by its external ID, including deleted entities.
    /// </summary>
    /// <param name="externalId">The external ID of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetByExternalIdIncludingDeletedAsync(Guid externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Business Incubator by its ID.
    /// </summary>
    /// <param name="id">The ID of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple Business Incubators by their IDs.
    /// </summary>
    /// <param name="ids">The collection of Business Incubator IDs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of Business Incubators that match the provided IDs.</returns>
    Task<List<Aggregates.BusinessIncubator.BusinessIncubator>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Business Incubator that contains a project with the specified external ID, including project questions and answers.
    /// </summary>
    /// <param name="projectExternalId">The external ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity that contains the project if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetByProjectExternalIdWithQuestionsAsync(Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Business Incubator by its project ID.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest review for a submission.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest review if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormReview?> GetLatestReviewBySubmissionIdAsync(long submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of pending submissions.
    /// </summary>
    /// <param name="projectIds">The project IDs to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of pending submissions.</returns>
    Task<int> GetPendingSubmissionsCountAsync(long[] projectIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending submissions for review.
    /// </summary>
    /// <param name="projectIds">The project IDs to filter by.</param>
    /// <param name="pageNumber">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending submissions.</returns>
    Task<List<Aggregates.BusinessIncubator.ProjectFormSubmission>> GetPendingSubmissionsForReviewAsync(long[] projectIds, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all form submissions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all submissions.</returns>
    Task<List<Aggregates.BusinessIncubator.ProjectFormSubmission>> GetAllSubmissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets form submissions by project IDs.
    /// </summary>
    /// <param name="projectIds">Array of project IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of submissions.</returns>
    Task<List<Aggregates.BusinessIncubator.ProjectFormSubmission>> GetSubmissionsByProjectIdsAsync(long[] projectIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all project answer option references by source answer option ID.
    /// </summary>
    /// <param name="sourceAnswerOptionId">The source answer option ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of project answer option references.</returns>
    Task<List<ProjectAnswerOptionReferenceDto>> GetProjectAnswerOptionReferencesBySourceIdAsync(long sourceAnswerOptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the existing block names and source IDs for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple of block names and source IDs.</returns>
    Task<(HashSet<string> Names, HashSet<long> SourceIds)> GetProjectBlockIdentifiersAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project by its external ID.
    /// </summary>
    /// <param name="projectExternalId">The external ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Project entity if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by its external ID including soft-deleted projects.
    /// </summary>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectByExternalIdIncludingDeletedAsync(Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by its internal ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectByIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets project questions for diagnosis grouped by blocks.
    /// </summary>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="questionPhase">The question phase filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of project questions grouped by blocks.</returns>
    Task<List<ProjectDiagnosisBlockDto>> GetProjectDiagnosisQuestionsAsync(Guid projectExternalId, int questionPhase, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets project form submissions for a specific user in a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of project form submissions for the user.</returns>
    Task<List<Aggregates.BusinessIncubator.ProjectFormSubmission>> GetProjectFormSubmissionsByUserAsync(long projectId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projects that reference a specific source form ID.
    /// </summary>
    /// <param name="sourceFormId">The source form ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of project IDs.</returns>
    Task<List<long>> GetProjectIdsBySourceFormIdAsync(long sourceFormId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project invitation by its token.
    /// </summary>
    /// <param name="token">The invitation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ProjectInvitation entity if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectInvitation?> GetProjectInvitationByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the project knowledge structure by its ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project knowledge structure if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectKnowledgeStructure?> GetProjectKnowledgeStructureAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projects in an incubator.
    /// </summary>
    /// <param name="incubatorId">The incubator ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of projects in the incubator.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetProjectsByIncubatorIdAsync(long incubatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all project module references by source module ID.
    /// </summary>
    /// <param name="sourceModuleId">The source module ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of project module references.</returns>
    Task<List<ProjectModuleReferenceDto>> GetProjectModuleReferencesBySourceIdAsync(long sourceModuleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all project question references by source question ID.
    /// </summary>
    /// <param name="sourceQuestionId">The source question ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of project question references.</returns>
    Task<List<ProjectQuestionReferenceDto>> GetProjectQuestionReferencesBySourceIdAsync(long sourceQuestionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple projects by their IDs.
    /// </summary>
    /// <param name="projectIds">The collection of project IDs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of projects that match the provided IDs.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetProjectsByIdsAsync(IEnumerable<long> projectIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projects that use a specific source form ID.
    /// </summary>
    /// <param name="sourceFormId">The source form ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of projects using the source form.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetProjectsBySourceFormAsync(long sourceFormId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projects that use a specific source module ID.
    /// </summary>
    /// <param name="sourceModuleId">The source module ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of projects using the source module.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetProjectsBySourceModuleAsync(long sourceModuleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projects associated with a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of projects the user is associated with.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetProjectsByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all project subject references by source subject ID.
    /// </summary>
    /// <param name="sourceSubjectId">The source subject ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of project subject references.</returns>
    Task<List<ProjectSubjectReferenceDto>> GetProjectSubjectReferencesBySourceIdAsync(long sourceSubjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets projects with knowledge structures from a business incubator.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="excludeProjectExternalId">The project external ID to exclude.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of projects with knowledge structures.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetProjectsWithKnowledgeStructureAsync(long businessIncubatorId, Guid excludeProjectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all project topic references by source topic ID.
    /// </summary>
    /// <param name="sourceTopicId">The source topic ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of project topic references.</returns>
    Task<List<ProjectTopicReferenceDto>> GetProjectTopicReferencesBySourceIdAsync(long sourceTopicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project with its blocks by external ID.
    /// </summary>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project with blocks if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithBlocksByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project with its blocks by internal ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project with blocks if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithBlocksByIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project with its form submissions by ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project with form submissions if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithFormSubmissionsAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project with its invitations by external ID.
    /// </summary>
    /// <param name="projectExternalId">The external ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Project entity with its invitations if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithInvitationsByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project with its knowledge structure by external ID.
    /// </summary>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project with knowledge structure if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithKnowledgeStructureByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project with its knowledge structure by internal ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project with knowledge structure if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithKnowledgeStructureByIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project with its users by project ID.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Project entity with its users if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithUsersAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project with its stages by external ID.
    /// </summary>
    /// <param name="projectExternalId">The external ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Project entity with its stages if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithStagesByExternalIdAsync(Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a review by its ID.
    /// </summary>
    /// <param name="reviewId">The review ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The review if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormReview?> GetReviewByIdAsync(long reviewId, CancellationToken cancellationToken = default);

    // Review-related methods
    /// <summary>
    /// Gets a review by submission ID.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The review if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormReview?> GetReviewBySubmissionIdAsync(long submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reviews for a submission.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of reviews for the submission.</returns>
    Task<List<Aggregates.BusinessIncubator.ProjectFormReview>> GetReviewsBySubmissionIdAsync(long submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets source modules by their IDs in batch.
    /// </summary>
    /// <param name="moduleIds">The module IDs to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of module ID to Module entity.</returns>
    Task<Dictionary<long, ValueObjects.Module>> GetSourceModulesAsync(List<long> moduleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project form submission by its ID.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project form submission if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormSubmission?> GetSubmissionByIdAsync(long submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a submission with all details for review.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The submission with details if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormSubmission?> GetSubmissionWithDetailsForReviewAsync(long submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a form submission by project ID, user ID, and phase.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="phase">The question phase.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The form submission if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormSubmission?> GetFormSubmissionAsync(
        long projectId,
        string userId,
        Enums.QuestionPhase phase,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a form submission by its ID.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The form submission if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormSubmission?> GetFormSubmissionByIdAsync(
        long submissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a form submission by its external ID.
    /// </summary>
    /// <param name="externalId">The external ID of the submission.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The form submission if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormSubmission?> GetFormSubmissionByExternalIdAsync(
        Guid externalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a form submission with related details by its external ID.
    /// </summary>
    /// <param name="externalId">The external ID of the submission.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The form submission with project and stage details if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormSubmission?> GetFormSubmissionWithDetailsByExternalIdAsync(
        Guid externalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new form submission.
    /// </summary>
    /// <param name="submission">The form submission to add.</param>
    void AddFormSubmission(Aggregates.BusinessIncubator.ProjectFormSubmission submission);

    /// <summary>
    /// Updates an existing form submission.
    /// </summary>
    /// <param name="submission">The form submission to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateFormSubmissionAsync(
        Aggregates.BusinessIncubator.ProjectFormSubmission submission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Business Incubator with its projects and knowledge structure by external ID.
    /// </summary>
    /// <param name="businessIncubatorExternalId">The external ID of the Business Incubator.</param>
    /// <param name="projectExternalId">The external ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity with its projects and knowledge structure if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetWithProjectAndKnowledgeStructureByExternalId(Guid businessIncubatorExternalId, Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Business Incubator with its project and related blocks by external ID.
    /// </summary>
    /// <param name="businessIncubatorExternalId">The external ID of the Business Incubator.</param>
    /// <param name="projectExternalId">The external ID of the project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity with its project and related blocks if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetWithProjectBlocksByExternalId(Guid businessIncubatorExternalId, Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Business Incubator by its external ID with its projects.
    /// </summary>
    /// <param name="externalId">The external ID of the Business Incubator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Business Incubator entity if found, otherwise null.</returns>
    /// <remarks>Includes the projects associated with the Business Incubator.</remarks>
    Task<Aggregates.BusinessIncubator.BusinessIncubator?> GetWithProjectsByExternalIdAsync(Guid externalId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks if a project with the given key exists within a business incubator.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectKey">The project key to check.</param>
    /// <param name="excludeProjectId">Optional project ID to exclude from the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a project with the key exists, false otherwise.</returns>
    Task<bool> ProjectExistsWithKeyAsync(long businessIncubatorId, string projectKey, long? excludeProjectId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a project with the given name exists within a business incubator.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectName">The project name to check.</param>
    /// <param name="excludeProjectId">Optional project ID to exclude from the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a project with the name exists, false otherwise.</returns>
    Task<bool> ProjectExistsWithNameAsync(long businessIncubatorId, string projectName, long? excludeProjectId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a project has a block with the specified name.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="blockName">The block name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a block with the name exists, otherwise false.</returns>
    Task<bool> ProjectHasBlockWithNameAsync(long projectId, string blockName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a project has a block with the specified source block ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="sourceBlockId">The source block ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a block with the source ID exists, otherwise false.</returns>
    Task<bool> ProjectHasBlockWithSourceIdAsync(long projectId, long sourceBlockId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing Business Incubator in the repository.
    /// </summary>
    /// <param name="businessIncubator">The Business Incubator entity to update.</param>
    void Update(Aggregates.BusinessIncubator.BusinessIncubator businessIncubator);
    /// <summary>
    /// Updates a project entity.
    /// </summary>
    /// <param name="project">The project to update.</param>
    void Update(Aggregates.BusinessIncubator.Project project);

    /// <summary>
    /// Updates a project invitation entity.
    /// </summary>
    /// <param name="invitation">The invitation to update.</param>
    void Update(Aggregates.BusinessIncubator.ProjectInvitation invitation);
    /// <summary>
    /// Updates a project asynchronously.
    /// </summary>
    /// <param name="project">The project to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Aggregates.BusinessIncubator.Project project, CancellationToken cancellationToken = default);

    // Sync-specific methods
    /// <summary>
    /// Updates multiple projects in batch.
    /// </summary>
    /// <param name="projects">The projects to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateProjectsAsync(List<Aggregates.BusinessIncubator.Project> projects, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a review.
    /// </summary>
    /// <param name="review">The review to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateReviewAsync(Aggregates.BusinessIncubator.ProjectFormReview review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a project form submission asynchronously.
    /// </summary>
    /// <param name="submission">The submission to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateSubmissionAsync(Aggregates.BusinessIncubator.ProjectFormSubmission submission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the given questions exist in the project and returns their validation data.
    /// </summary>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="questionIds">The list of question IDs to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary of validation data keyed by question ID.</returns>
    Task<Dictionary<long, ProjectQuestionValidationDto>> ValidateProjectQuestionsAsync(Guid projectExternalId, List<long> questionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project with its stages by internal ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The project with stages if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.Project?> GetProjectWithStagesAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is a participant of a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is a participant, otherwise false.</returns>
    Task<bool> IsUserProjectParticipantAsync(long projectId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a feedback by its ID.
    /// </summary>
    /// <param name="feedbackId">The feedback ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The feedback if found, otherwise null.</returns>
    Task<Aggregates.BusinessIncubator.ProjectFormFeedback?> GetFeedbackByIdAsync(long feedbackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets feedback for a submission with replies.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of feedback with replies.</returns>
    Task<List<Aggregates.BusinessIncubator.ProjectFormFeedback>> GetFeedbackWithRepliesForSubmissionAsync(long submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new feedback.
    /// </summary>
    /// <param name="feedback">The feedback to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    Task AddFeedbackAsync(Aggregates.BusinessIncubator.ProjectFormFeedback feedback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing feedback.
    /// </summary>
    /// <param name="feedback">The feedback to update.</param>
    void UpdateFeedback(Aggregates.BusinessIncubator.ProjectFormFeedback feedback);

    /// <summary>
    /// Gets project questions with answer options for a specific project and phase.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="phase">The question phase.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of question ID to ProjectQuestion entity with answer options.</returns>
    Task<Dictionary<long, Aggregates.BusinessIncubator.ProjectQuestion>> GetProjectQuestionsWithAnswerOptionsAsync(
        long projectId,
        Enums.QuestionPhase phase,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets answer options by their IDs with all metadata.
    /// </summary>
    /// <param name="answerOptionIds">List of answer option IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of ProjectAnswerOption entities with full metadata.</returns>
    Task<List<Aggregates.BusinessIncubator.ProjectAnswerOption>> GetAnswerOptionsByIdsAsync(
        List<long> answerOptionIds,
        CancellationToken cancellationToken = default);

    // NOTE: Report methods have been moved to IReportsRepository for proper DDD separation

    /// <summary>
    /// Gets projects within specified geohash prefixes and bounding box for proximity searches.
    /// </summary>
    /// <param name="geohashPrefixes">Set of geohash prefixes to search within.</param>
    /// <param name="minLat">Minimum latitude of bounding box.</param>
    /// <param name="maxLat">Maximum latitude of bounding box.</param>
    /// <param name="minLon">Minimum longitude of bounding box.</param>
    /// <param name="maxLon">Maximum longitude of bounding box.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of projects with location data that match the criteria.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetProjectsInGeohashesAsync(
        HashSet<string> geohashPrefixes,
        decimal minLat,
        decimal maxLat,
        decimal minLon,
        decimal maxLon,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing project interest for a user.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="interestType">The type of interest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The interest record if found, otherwise null.</returns>
    Task<object?> GetProjectInterestAsync(
        long projectId,
        string userId,
        string interestType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a project interest.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The user ID (optional).</param>
    /// <param name="sessionId">The session ID for anonymous users (optional).</param>
    /// <param name="interestType">The type of interest.</param>
    /// <param name="observerLatitude">Observer's latitude (optional).</param>
    /// <param name="observerLongitude">Observer's longitude (optional).</param>
    /// <param name="distance">Distance in km (optional).</param>
    /// <param name="userAgent">User agent string (optional).</param>
    /// <param name="ipAddress">IP address (optional).</param>
    /// <param name="referrerUrl">Referrer URL (optional).</param>
    /// <param name="createdAt">Creation timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordProjectInterestAsync(
        long projectId,
        string? userId,
        string? sessionId,
        string interestType,
        decimal? observerLatitude,
        decimal? observerLongitude,
        double? distance,
        string? userAgent,
        string? ipAddress,
        string? referrerUrl,
        DateTime createdAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active projects with their stages for homepage display.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active projects with stages loaded.</returns>
    Task<List<Aggregates.BusinessIncubator.Project>> GetActiveProjectsWithStagesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for project diagnosis blocks.
/// </summary>
public record ProjectDiagnosisBlockDto(long Id, string Title, List<ProjectDiagnosisQuestionDto> Questions);

/// <summary>
/// DTO for project diagnosis questions.
/// </summary>
public record ProjectDiagnosisQuestionDto(long Id, int AnswerType, string Text, bool IsTextCustomized, List<ProjectDiagnosisAnswerOptionDto> Options);

/// <summary>
/// DTO for project diagnosis answer options.
/// </summary>
public record ProjectDiagnosisAnswerOptionDto(long Id, string Text, string? FollowUpQuestionText, bool IsTextCustomized);

/// <summary>
/// DTO for validating project question answers.
/// </summary>
public record ProjectQuestionValidationDto(long QuestionId, int AnswerType, List<long> ValidAnswerOptionIds);

/// <summary>
/// DTO for project module references.
/// </summary>
public record ProjectModuleReferenceDto(long ProjectId, long ModuleId);

/// <summary>
/// DTO for project topic references.
/// </summary>
public record ProjectTopicReferenceDto(long ProjectId, long TopicId);

/// <summary>
/// DTO for project subject references.
/// </summary>
public record ProjectSubjectReferenceDto(long ProjectId, long SubjectId);

/// <summary>
/// DTO for project question references.
/// </summary>
public record ProjectQuestionReferenceDto(long ProjectId, long QuestionId);

/// <summary>
/// DTO for project answer option references.
/// </summary>
public record ProjectAnswerOptionReferenceDto(long ProjectId, long AnswerOptionId);
