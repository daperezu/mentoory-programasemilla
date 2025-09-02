using LinaSys.BusinessIncubator.Application.Project.Commands;
using LinaSys.BusinessIncubator.Application.Project.Queries;
using LinaSys.Orchestration.Application.BusinessIncubator.Commands;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models.Invitations;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

public class InvitationsController(ILogger<InvitationsController> logger, MediatorExecutor mediatorExecutor)
    : AuthorizedBaseController(logger, mediatorExecutor)
{
    [HttpGet("invitations/accept/{token}")]
    public async Task<IActionResult> Accept(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return NotFound();
        }

        var query = new GetProjectInvitationByTokenQuery(token);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess)
        {
            return View("InvitationNotFound");
        }

        var invitation = result.Value!;

        // Check if invitation is still valid
        if (invitation.Status != BusinessIncubator.Domain.Enums.ProjectInvitationStatus.Pending || invitation.IsExpired)
        {
            var model = new InvitationExpiredViewModel
            {
                ProjectName = invitation.ProjectName,
                Email = invitation.Email,
                FullName = invitation.FullName,
            };
            return View("InvitationExpired", model);
        }

        var viewModel = new AcceptInvitationViewModel
        {
            Token = token,
            Email = invitation.Email,
            FullName = invitation.FullName,
            ProjectName = invitation.ProjectName,
            ExpiresAt = invitation.ExpiresAt,
        };

        return View(viewModel);
    }

    [HttpPost("invitations/accept/{token}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(string token, AcceptInvitationViewModel model)
    {
        if (string.IsNullOrWhiteSpace(token) || token != model.Token)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var command = new AcceptProjectInvitationOrchestrationCommand(token, model.NewPassword);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

            if (result.IsSuccess)
            {
                this.SetSuccessToast("¡Bienvenido! Tu cuenta ha sido activada exitosamente y ahora tienes acceso al proyecto.");
                return RedirectToAction("Login", "Account", new { area = "Identity", email = model.Email });
            }

            MapErrorsToModelStateAndSetErrorToast<AcceptProjectInvitationOrchestrationCommand>(result);
        }

        // Reload invitation data for the view
        var query = new GetProjectInvitationByTokenQuery(token);
        var invitationResult = await MediatorExecutor.SendAndLogIfFailureAsync(query);

        if (invitationResult.IsSuccess)
        {
            var invitation = invitationResult.Value!;
            model.Email = invitation.Email;
            model.FullName = invitation.FullName;
            model.ProjectName = invitation.ProjectName;
            model.ExpiresAt = invitation.ExpiresAt;
        }

        return View(model);
    }

    [HttpPost("invitations/decline/{token}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return NotFound();
        }

        var command = new ProcessProjectInvitationCommand(token, InvitationAction.Decline);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            return View("InvitationDeclined");
        }

        this.SetErrorToast("No se pudo procesar tu respuesta. Por favor, intenta nuevamente.");
        return RedirectToAction("Accept", new { token });
    }
}
