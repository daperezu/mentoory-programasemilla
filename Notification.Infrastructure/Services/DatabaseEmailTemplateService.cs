using System.Text.RegularExpressions;
using LinaSys.Notification.Application.Templates;
using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Shared.Application.TimeProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Infrastructure.Services;

/// <summary>
/// Service that renders email templates from database with variable substitution.
/// </summary>
public class DatabaseEmailTemplateService(
    IEmailTemplateRepository templateRepository,
    IConfiguration configuration,
    ITimeProvider timeProvider,
    ILogger<DatabaseEmailTemplateService> logger) : IEmailTemplateService
{
    private static readonly Regex VariableRegex = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);
    private readonly string _applicationName = configuration["Application:Name"] ?? "LinaSys";
    private readonly string _logoUrl = configuration["Application:LogoUrl"] ?? "https://linasys.com/assets/logo-full-color-120.png";
    private readonly string _websiteUrl = configuration["Application:WebsiteUrl"] ?? "https://linasys.com";
    private readonly string _supportEmail = configuration["Application:SupportEmail"] ?? "soporte@linasys.com";

    public string GenerateAccountCreationEmail(
        string fullName,
        string email,
        string temporaryPassword,
        string activationLink,
        string projectName)
    {
        var variables = new Dictionary<string, string>
        {
            ["FullName"] = fullName,
            ["Email"] = email,
            ["TemporaryPassword"] = temporaryPassword,
            ["ActivationLink"] = activationLink,
            ["ProjectName"] = projectName
        };

        return RenderTemplate("account-creation-with-project", variables);
    }

    public string GenerateProjectInvitationEmail(
        string fullName,
        string projectName,
        string invitationLink)
    {
        var variables = new Dictionary<string, string>
        {
            ["FullName"] = fullName,
            ["ProjectName"] = projectName,
            ["InvitationLink"] = invitationLink
        };

        return RenderTemplate("project-invitation", variables);
    }

    public string GenerateInvitationReminderEmail(
        string fullName,
        string projectName,
        string invitationLink,
        int daysRemaining)
    {
        var variables = new Dictionary<string, string>
        {
            ["FullName"] = fullName,
            ["ProjectName"] = projectName,
            ["InvitationLink"] = invitationLink,
            ["DaysRemaining"] = daysRemaining.ToString()
        };

        return RenderTemplate("invitation-reminder", variables);
    }

    public string GenerateFormSubmissionConfirmationEmail(
        string participantName,
        string projectName,
        string submissionDateTime,
        string submissionId)
    {
        var variables = new Dictionary<string, string>
        {
            ["ParticipantName"] = participantName,
            ["ProjectName"] = projectName,
            ["SubmissionDateTime"] = submissionDateTime,
            ["SubmissionId"] = submissionId
        };

        return RenderTemplate("form-submission-confirmation", variables);
    }

    public string GenerateFormApprovedEmail(
        string participantName,
        string projectName,
        string projectDashboardUrl)
    {
        var variables = new Dictionary<string, string>
        {
            ["ParticipantName"] = participantName,
            ["ProjectName"] = projectName,
            ["ProjectDashboardUrl"] = projectDashboardUrl
        };

        return RenderTemplate("form-approved", variables);
    }

    public string GenerateFormRejectedEmail(
        string participantName,
        string projectName,
        string rejectionReason,
        string formEditUrl)
    {
        var variables = new Dictionary<string, string>
        {
            ["ParticipantName"] = participantName,
            ["ProjectName"] = projectName,
            ["RejectionReason"] = rejectionReason,
            ["FormEditUrl"] = formEditUrl
        };

        return RenderTemplate("form-rejected", variables);
    }

    public string GenerateReviewRequestEmail(
        string participantName,
        string projectName,
        string reviewComments,
        string formEditUrl,
        string reviewerName,
        string reviewDeadline)
    {
        var variables = new Dictionary<string, string>
        {
            ["ParticipantName"] = participantName,
            ["ProjectName"] = projectName,
            ["ReviewComments"] = reviewComments,
            ["FormEditUrl"] = formEditUrl,
            ["ReviewerName"] = reviewerName,
            ["ReviewDeadline"] = reviewDeadline
        };

        return RenderTemplate("review-request", variables);
    }

    public string GenerateFormSubmissionAdminNotificationEmail(
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
        int averageReviewTime)
    {
        var variables = new Dictionary<string, string>
        {
            ["ReviewerName"] = reviewerName,
            ["ProjectName"] = projectName,
            ["ParticipantName"] = participantName,
            ["ParticipantEmail"] = participantEmail,
            ["SubmissionDateTime"] = submissionDateTime,
            ["SubmissionId"] = submissionId,
            ["ReviewDashboardUrl"] = reviewDashboardUrl,
            ["PendingCount"] = pendingCount.ToString(),
            ["ReviewedToday"] = reviewedToday.ToString(),
            ["TotalSubmissions"] = totalSubmissions.ToString(),
            ["AverageReviewTime"] = averageReviewTime.ToString()
        };

        return RenderTemplate("form-submission-admin-notification", variables);
    }

    public string GenerateWelcomeEmail(
        string fullName,
        string loginCredential,
        string? temporaryPassword,
        string loginUrl,
        bool emailConfirmed = true,
        string? confirmationUrl = null)
    {
        var variables = new Dictionary<string, string>
        {
            ["FullName"] = fullName,
            ["Email"] = loginCredential, // This is actually the identification now
            ["LoginUrl"] = loginUrl
        };

        // If email is not confirmed, add confirmation URL and use confirmation template
        if (!emailConfirmed && !string.IsNullOrEmpty(confirmationUrl))
        {
            variables["ConfirmationUrl"] = confirmationUrl;
            if (!string.IsNullOrEmpty(temporaryPassword))
            {
                variables["TemporaryPassword"] = temporaryPassword;
            }

            return RenderTemplate("welcome-email-confirm-required", variables);
        }

        // Use different template based on whether there's a temporary password
        if (!string.IsNullOrEmpty(temporaryPassword))
        {
            variables["TemporaryPassword"] = temporaryPassword;
            return RenderTemplate("welcome-email-with-password", variables);
        }
        else
        {
            return RenderTemplate("welcome-email", variables);
        }
    }

    public string GeneratePasswordResetEmail(
        string fullName,
        string email,
        string resetLink,
        string requestDateTime,
        string requestLocation)
    {
        var variables = new Dictionary<string, string>
        {
            ["FullName"] = fullName,
            ["Email"] = email,
            ["ResetLink"] = resetLink,
            ["RequestDateTime"] = requestDateTime,
            ["RequestLocation"] = requestLocation
        };

        return RenderTemplate("password-reset", variables);
    }

    public string GenerateEmailChangeVerificationEmail(
        string fullName,
        string oldEmail,
        string newEmail,
        string verificationLink)
    {
        var variables = new Dictionary<string, string>
        {
            ["FullName"] = fullName,
            ["OldEmail"] = oldEmail,
            ["NewEmail"] = newEmail,
            ["VerificationLink"] = verificationLink
        };

        return RenderTemplate("email-change-verification", variables);
    }

    public string GenerateIdentificationChangeNotificationEmail(
        string fullName,
        string oldIdentification,
        string newIdentification,
        string changeDateTime,
        string requestedBy,
        string supportUrl)
    {
        var variables = new Dictionary<string, string>
        {
            ["FullName"] = fullName,
            ["OldIdentification"] = oldIdentification,
            ["NewIdentification"] = newIdentification,
            ["ChangeDateTime"] = changeDateTime,
            ["RequestedBy"] = requestedBy,
            ["SupportUrl"] = supportUrl
        };

        return RenderTemplate("identification-change-notification", variables);
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        return VariableRegex.Replace(template, match =>
        {
            var variableName = match.Groups[1].Value;
            return variables.TryGetValue(variableName, out var value) ? value : match.Value;
        });
    }

    private string RenderTemplate(string templateKey, Dictionary<string, string> variables)
    {
        try
        {
            // Get template from database
            var template = templateRepository.GetByKeyAsync(templateKey).GetAwaiter().GetResult();
            if (template is null)
            {
                logger.LogWarning("Email template '{TemplateKey}' not found in database", templateKey);
                return GenerateFallbackEmail(
                    variables.GetValueOrDefault("FullName", "Usuario"),
                    $"Notificación de {_applicationName}",
                    "No se pudo cargar la plantilla de correo. Por favor contacte a soporte.");
            }

            // Add system variables
            variables["ApplicationName"] = _applicationName;
            variables["LogoUrl"] = _logoUrl;
            variables["WebsiteUrl"] = _websiteUrl;
            variables["SupportEmail"] = _supportEmail;
            variables["CurrentYear"] = timeProvider.Now.Year.ToString();

            // Replace variables in subject
            var subject = ReplaceVariables(template.Subject, variables);

            // Replace variables in body
            var body = ReplaceVariables(template.BodyHtml, variables);

            return body;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rendering email template '{TemplateKey}'", templateKey);
            return GenerateFallbackEmail(
                variables.GetValueOrDefault("FullName", "Usuario"),
                $"Notificación de {_applicationName}",
                "Ocurrió un error al generar el correo. Por favor contacte a soporte.");
        }
    }

    private string GenerateFallbackEmail(string fullName, string subject, string message)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{subject}</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2>Hola {fullName},</h2>
        <p>{message}</p>
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        <p style='font-size: 12px; color: #666;'>
            Este es un correo automático de {_applicationName}.<br>
            Si necesita ayuda, contacte a: {_supportEmail}
        </p>
    </div>
</body>
</html>";
    }
}