namespace LinaSys.Shared.Application.Services;

/// <summary>
/// Service for generating application URLs in a clean architecture compliant way.
/// This abstraction allows the application layer to generate URLs without knowing web-specific details.
/// </summary>
public interface IApplicationUrlService
{
    /// <summary>
    /// Gets the URL for the form review dashboard.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator external ID.</param>
    /// <param name="projectId">The project external ID.</param>
    /// <returns>The absolute URL for form review.</returns>
    string GetFormReviewUrl(Guid businessIncubatorId, Guid projectId);

    /// <summary>
    /// Gets the URL for the project dashboard.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator external ID.</param>
    /// <param name="projectId">The project external ID.</param>
    /// <returns>The absolute URL for the project dashboard.</returns>
    string GetProjectDashboardUrl(Guid businessIncubatorId, Guid projectId);

    /// <summary>
    /// Gets the URL for editing a participant form.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator external ID.</param>
    /// <param name="projectId">The project external ID.</param>
    /// <returns>The absolute URL for editing the participant form.</returns>
    string GetParticipantFormUrl(Guid businessIncubatorId, Guid projectId);

    /// <summary>
    /// Gets the URL for accepting a project invitation.
    /// </summary>
    /// <param name="invitationToken">The invitation token.</param>
    /// <returns>The absolute URL for accepting the invitation.</returns>
    string GetInvitationAcceptanceUrl(string invitationToken);

    /// <summary>
    /// Gets the URL for resetting a password.
    /// </summary>
    /// <param name="resetToken">The password reset token.</param>
    /// <returns>The absolute URL for the password reset page.</returns>
    string GetPasswordResetUrl(string resetToken);

    /// <summary>
    /// Gets the URL for verifying an email change.
    /// </summary>
    /// <param name="verificationToken">The email verification token.</param>
    /// <returns>The absolute URL for verifying the email change.</returns>
    string GetEmailChangeVerificationUrl(string verificationToken);

    /// <summary>
    /// Gets the URL for editing a form submission.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="submissionId">The submission ID.</param>
    /// <returns>The absolute URL for editing the form submission.</returns>
    string GetFormEditUrl(long projectId, long submissionId);

    /// <summary>
    /// Gets the URL for the participant project dashboard.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The absolute URL for the participant project dashboard.</returns>
    string GetParticipantProjectDashboardUrl(long projectId);

    /// <summary>
    /// Gets the URL for editing a form submission (without project context).
    /// </summary>
    /// <param name="submissionId">The submission ID.</param>
    /// <returns>The absolute URL for editing the form submission.</returns>
    string GetFormEditUrl(long submissionId);

    /// <summary>
    /// Gets the URL for the login page.
    /// </summary>
    /// <returns>The absolute URL for the login page.</returns>
    string GetLoginUrl();

    /// <summary>
    /// Gets the URL for email confirmation.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="token">The confirmation token.</param>
    /// <returns>The absolute URL for email confirmation.</returns>
    string GetEmailConfirmationUrl(string userId, string token);

    /// <summary>
    /// Gets the URL for the logout page.
    /// </summary>
    /// <returns>The absolute URL for the logout page.</returns>
    string GetLogoutUrl();

    /// <summary>
    /// Gets the URL for the coordinator participant management page.
    /// </summary>
    /// <returns>The absolute URL for the participant management page.</returns>
    string GetCoordinatorParticipantManagementUrl();

    /// <summary>
    /// Gets the URL for the participant dashboard.
    /// </summary>
    /// <returns>The absolute URL for the participant dashboard.</returns>
    string GetParticipantDashboardUrl();
}