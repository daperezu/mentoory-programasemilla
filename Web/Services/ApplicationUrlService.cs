using System.Text;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace LinaSys.Web.Services;

/// <summary>
/// Web implementation of the application URL service.
/// Generates absolute URLs for various application endpoints.
/// </summary>
public class ApplicationUrlService(
    IUrlHelperFactory urlHelperFactory,
    IActionContextAccessor actionContextAccessor,
    IConfiguration configuration) : IApplicationUrlService
{

    /// <inheritdoc/>
    public string GetFormReviewUrl(Guid businessIncubatorId, Guid projectId)
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Action(
            action: "Index",
            controller: "FormReview",
            values: new { area = "BusinessIncubators", businessIncubatorExternalId = businessIncubatorId, projectExternalId = projectId });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetProjectDashboardUrl(Guid businessIncubatorId, Guid projectId)
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Action(
            action: "Dashboard",
            controller: "Projects",
            values: new { area = "BusinessIncubators", businessIncubatorExternalId = businessIncubatorId, projectExternalId = projectId });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetParticipantFormUrl(Guid businessIncubatorId, Guid projectId)
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Action(
            action: "Index",
            controller: "ParticipantForm",
            values: new
            {
                area = "BusinessIncubators",
                businessIncubatorExternalId = businessIncubatorId,
                projectExternalId = projectId
            });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetInvitationAcceptanceUrl(string invitationToken)
    {
        // This might be a specific route or page, adjust as needed
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}/invitations/accept/{invitationToken}";
    }

    /// <inheritdoc/>
    public string GetPasswordResetUrl(string resetToken)
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Page(
            "/Account/ResetPassword",
            values: new { area = "Identity", code = resetToken });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetEmailChangeVerificationUrl(string verificationToken)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}/Account/VerifyEmailChange?token={verificationToken}";
    }

    /// <inheritdoc/>
    public string GetFormEditUrl(long projectId, long submissionId)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}/Participant/Project/{projectId}/Form/Edit/{submissionId}";
    }

    /// <inheritdoc/>
    public string GetParticipantProjectDashboardUrl(long projectId)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}/Participant/Project/{projectId}/Dashboard";
    }

    /// <inheritdoc/>
    public string GetFormEditUrl(long submissionId)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}/Participant/Form/Edit/{submissionId}";
    }

    /// <inheritdoc/>
    public string GetLoginUrl()
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Page(
            "/Account/Login",
            values: new { area = "Identity" });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetEmailConfirmationUrl(string userId, string token)
    {
        // Encode the token for URL safety
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Page(
            "/Account/ConfirmEmail",
            values: new { area = "Identity", userId = userId, code = encodedToken });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetLogoutUrl()
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Page(
            "/Account/Logout",
            values: new { area = "Identity" });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetCoordinatorParticipantManagementUrl()
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Action(
            action: "Index",
            controller: "Participant",
            values: new { area = "Coordination" });

        return GetAbsoluteUrl(relativeUrl);
    }

    /// <inheritdoc/>
    public string GetParticipantDashboardUrl()
    {
        var urlHelper = GetUrlHelper();
        var relativeUrl = urlHelper.Action(
            action: "Index",
            controller: "Dashboard",
            values: new { area = "Participant" });

        return GetAbsoluteUrl(relativeUrl);
    }

    private IUrlHelper GetUrlHelper()
    {
        var actionContext = actionContextAccessor.ActionContext;
        if (actionContext is null)
        {
            // Fallback for when called outside of HTTP context (e.g., background jobs)
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString(GetBaseUrl().Replace("https://", string.Empty).Replace("http://", string.Empty))
                }
            };
            actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        return urlHelperFactory.GetUrlHelper(actionContext);
    }

    private string GetAbsoluteUrl(string? relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl))
        {
            return GetBaseUrl();
        }

        var baseUrl = GetBaseUrl();
        if (relativeUrl.StartsWith("/"))
        {
            return $"{baseUrl}{relativeUrl}";
        }

        return $"{baseUrl}/{relativeUrl}";
    }

    private string GetBaseUrl()
    {
        return configuration["Application:BaseUrl"] ?? "https://localhost:5038";
    }
}
