using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Partial class for managing form submissions within a project.
/// </summary>
public partial class Project
{
    private readonly List<ProjectFormSubmission> _formSubmissions = [];

    /// <summary>
    /// Gets the read-only collection of form submissions.
    /// </summary>
    public IReadOnlyCollection<ProjectFormSubmission> FormSubmissions => _formSubmissions.AsReadOnly();

    /// <summary>
    /// Starts a new form submission for a participant.
    /// </summary>
    /// <param name="participantUserId">The participant user ID.</param>
    /// <param name="startedAt">The timestamp when the form was started.</param>
    /// <returns>The created form submission.</returns>
    /// <exception cref="InvalidOperationException">If the participant already has an active submission.</exception>
    /// <remarks>
    /// Access verification should be done at the application layer using UserProjectAccess from Auth domain.
    /// </remarks>
    public ProjectFormSubmission StartFormSubmission(string participantUserId, DateTime startedAt)
    {
        EnsureNotDeleted();

        // Note: Access verification is handled at the application layer via UserProjectAccess

        // Check if participant already has a submission
        var existingSubmission = _formSubmissions.FirstOrDefault(s =>
            s.ParticipantUserId == participantUserId);

        if (existingSubmission is not null)
        {
            // If rejected, allow to edit again
            if (existingSubmission.Status == ProjectFormSubmissionStatus.Rejected)
            {
                existingSubmission.EnableEditingAfterRejection();
            }

            // Otherwise, return existing submission
            return existingSubmission;
        }

        // Get current form schema version from knowledge structure
        var currentVersion = ProjectKnowledgeStructure?.CurrentVersion ?? 1;

        // Create new submission
        var submission = ProjectFormSubmission.Create(Id, participantUserId, currentVersion, startedAt);
        _formSubmissions.Add(submission);

        return submission;
    }

    /// <summary>
    /// Gets a form submission by ID.
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <returns>The form submission if found.</returns>
    public ProjectFormSubmission? GetFormSubmission(long submissionId)
    {
        return _formSubmissions.FirstOrDefault(s => s.Id == submissionId);
    }

    /// <summary>
    /// Gets all form submissions for a specific participant.
    /// </summary>
    /// <param name="participantUserId">The participant user ID.</param>
    /// <returns>Collection of form submissions.</returns>
    public IEnumerable<ProjectFormSubmission> GetParticipantSubmissions(string participantUserId)
    {
        return _formSubmissions.Where(s => s.ParticipantUserId == participantUserId);
    }

    /// <summary>
    /// Gets all form submissions pending review.
    /// </summary>
    /// <returns>Collection of submitted form submissions.</returns>
    public IEnumerable<ProjectFormSubmission> GetSubmissionsForReview()
    {
        return _formSubmissions.Where(s => s.Status == ProjectFormSubmissionStatus.Submitted);
    }

    /// <summary>
    /// Checks if a participant has access to fill forms.
    /// </summary>
    /// <param name="participantUserId">The participant user ID.</param>
    /// <returns>True if the participant has access.</returns>
    public bool HasFormAccess(string participantUserId)
    {
        return _projectInvitations.Any(i =>
            i.IdentificationNumber == participantUserId &&
            i.Status == ProjectInvitationStatus.Accepted);
    }

    /// <summary>
    /// Gets or creates a form submission on behalf of a participant.
    /// </summary>
    /// <param name="participantUserId">The participant user ID for whom the form is being filled.</param>
    /// <param name="submittedByUserId">The user ID who is filling the form on behalf.</param>
    /// <param name="currentDate">The current date/time.</param>
    /// <returns>The form submission (existing draft or new on-behalf submission).</returns>
    public ProjectFormSubmission GetOrCreateFormSubmissionOnBehalf(
        string participantUserId,
        string submittedByUserId,
        DateTime currentDate)
    {
        EnsureNotDeleted();

        // Check if there's an existing draft submission
        var existingSubmission = _formSubmissions.FirstOrDefault(s =>
            s.ParticipantUserId == participantUserId &&
            s.Status == ProjectFormSubmissionStatus.Draft);

        if (existingSubmission is not null)
        {
            // Update to on-behalf mode if not already
            if (existingSubmission.SubmissionMode != SubmissionMode.OnBehalf)
            {
                // For now, return the existing submission
                // A separate method could be added to update submission mode if needed
                return existingSubmission;
            }

            return existingSubmission;
        }

        // Get current stage and phase
        var currentStage = GetCurrentStage(currentDate);
        var phase = currentStage is not null
            ? ProjectFormSubmission.GetPhaseForStage(currentStage.Type)
            : QuestionPhase.Start;

        // Get current form schema version from knowledge structure
        var currentVersion = ProjectKnowledgeStructure?.CurrentVersion ?? 1;

        // Create new on-behalf submission
        var submission = ProjectFormSubmission.CreateOnBehalf(
            Id,
            participantUserId,
            submittedByUserId,
            currentVersion,
            phase,
            currentStage?.Id,
            currentDate);

        _formSubmissions.Add(submission);
        return submission;
    }
}
