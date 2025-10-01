namespace LinaSys.Notification.Application.Templates;

/// <summary>
/// Service for generating email content from templates.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Generates an account creation email with project invitation.
    /// </summary>
    /// <returns></returns>
    string GenerateAccountCreationEmail(
        string fullName,
        string email,
        string temporaryPassword,
        string activationLink,
        string projectName);

    /// <summary>
    /// Generates a project invitation email.
    /// </summary>
    /// <returns></returns>
    string GenerateProjectInvitationEmail(
        string fullName,
        string projectName,
        string invitationLink);

    /// <summary>
    /// Generates an invitation reminder email.
    /// </summary>
    /// <returns></returns>
    string GenerateInvitationReminderEmail(
        string fullName,
        string projectName,
        string invitationLink,
        int daysRemaining);

    /// <summary>
    /// Generates a form submission confirmation email.
    /// </summary>
    /// <returns></returns>
    string GenerateFormSubmissionConfirmationEmail(
        string participantName,
        string projectName,
        string submissionDateTime,
        string submissionId);

    /// <summary>
    /// Generates a form approved email.
    /// </summary>
    /// <returns></returns>
    string GenerateFormApprovedEmail(
        string participantName,
        string projectName,
        string projectDashboardUrl);

    /// <summary>
    /// Generates a form rejected email.
    /// </summary>
    /// <returns></returns>
    string GenerateFormRejectedEmail(
        string participantName,
        string projectName,
        string rejectionReason,
        string formEditUrl);

    /// <summary>
    /// Generates a review request email.
    /// </summary>
    /// <returns></returns>
    string GenerateReviewRequestEmail(
        string participantName,
        string projectName,
        string reviewComments,
        string formEditUrl,
        string reviewerName,
        string reviewDeadline);

    /// <summary>
    /// Generates a form submission admin notification email.
    /// </summary>
    /// <returns></returns>
    string GenerateFormSubmissionAdminNotificationEmail(
        string reviewerName,
        string projectName,
        string participantName,
        string participantEmail,
        string submissionDateTime,
        string submissionId,
        string reviewDashboardUrl,
        int pendingCount,
        int reviewedToday,
        int totalSubmissions,
        int averageReviewTime);

    /// <summary>
    /// Generates a welcome email.
    /// </summary>
    /// <param name="fullName">User's full name.</param>
    /// <param name="loginCredential">User's login credential (identification).</param>
    /// <param name="temporaryPassword">Temporary password (null if user set their own).</param>
    /// <param name="loginUrl">URL to the login page.</param>
    /// <param name="emailConfirmed">Whether the email is already confirmed.</param>
    /// <param name="confirmationUrl">URL for email confirmation (null if already confirmed).</param>
    /// <returns></returns>
    string GenerateWelcomeEmail(
        string fullName,
        string loginCredential,
        string? temporaryPassword,
        string loginUrl,
        bool emailConfirmed = true,
        string? confirmationUrl = null);

    /// <summary>
    /// Generates a password reset email.
    /// </summary>
    /// <returns></returns>
    string GeneratePasswordResetEmail(
        string fullName,
        string email,
        string resetLink,
        string requestDateTime,
        string requestLocation);

    /// <summary>
    /// Generates an email change verification email.
    /// </summary>
    /// <returns></returns>
    string GenerateEmailChangeVerificationEmail(
        string fullName,
        string oldEmail,
        string newEmail,
        string verificationLink);

    /// <summary>
    /// Generates an identification change notification email.
    /// </summary>
    /// <returns></returns>
    string GenerateIdentificationChangeNotificationEmail(
        string fullName,
        string oldIdentification,
        string newIdentification,
        string changeDateTime,
        string requestedBy,
        string supportUrl);

    /// <summary>
    /// Generates a project stage activated email.
    /// </summary>
    /// <returns></returns>
    string GenerateProjectStageActivatedEmail(
        string participantName,
        string projectName,
        string stageName,
        string stageType,
        DateTime startDate,
        DateTime endDate,
        string dashboardUrl);
}