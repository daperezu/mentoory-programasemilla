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
}
