using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Approve;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Reject;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetSubmissionsForReview;
using LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;
using LinaSys.Web.Controllers;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

/// <summary>
/// Controller for reviewing participant form submissions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FormReviewController"/> class.
/// </remarks>
/// <param name="mediator">The mediator executor.</param>
/// <param name="repository">The business incubator repository.</param>
[Area("BusinessIncubators")]
[Route("[area]/{businessIncubatorExternalId:guid}/Projects/{projectExternalId:guid}/[controller]")]
public class FormReviewController(
    ILogger<FormReviewController> logger,
    MediatorExecutor mediator,
    BusinessIncubator.Domain.Repositories.IBusinessIncubatorRepository repository)
    : AuthorizedBaseController(logger, mediator)
{

    /// <summary>
    /// Displays the form review dashboard.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId)
    {
        // Verify user has review access to the project
        var project = await repository.GetProjectByExternalIdAsync(projectExternalId);
        if (project is null)
        {
            return NotFound();
        }

        // For now, check if user is part of the business incubator
        // TODO: Implement proper role-based access (Admin/Reviewer)
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var model = new FormReviewViewModel
        {
            BusinessIncubatorExternalId = businessIncubatorExternalId,
            ProjectExternalId = projectExternalId
        };

        return View(model);
    }

    /// <summary>
    /// Gets submissions for review via AJAX.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Route("GetSubmissions")]
    public async Task<IActionResult> GetSubmissions(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromBody] GetSubmissionsRequest request)
    {
        var query = new GetSubmissionsForReviewQuery
        {
            ProjectExternalId = projectExternalId,
            OnlyPending = request.OnlyPending
        };

        var result = await MediatorExecutor.SendOrThrowAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Displays detailed submission for review.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet]
    [Route("Details/{submissionId:long}")]
    public async Task<IActionResult> Details(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromRoute] long submissionId)
    {
        // Verify user has review access to the project
        var project = await repository.GetProjectByExternalIdAsync(projectExternalId);
        if (project is null)
        {
            return NotFound();
        }

        // For now, check if user is part of the business incubator
        // TODO: Implement proper role-based access (Admin/Reviewer)
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        // TODO: Load submission details with all answers
        var model = new SubmissionDetailsViewModel
        {
            BusinessIncubatorExternalId = businessIncubatorExternalId,
            ProjectExternalId = projectExternalId,
            SubmissionId = submissionId
        };

        return View(model);
    }

    /// <summary>
    /// Approves a form submission.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Route("Approve")]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromBody] ApproveSubmissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new ApproveFormSubmissionCommand
        {
            ProjectId = request.ProjectId,
            SubmissionId = request.SubmissionId,
            ApproverUserId = userId
        };

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al procesar la solicitud."] });
        }

        return Ok(new
        {
            success = true,
            message = "Formulario aprobado exitosamente. Se generarán las respuestas de diagnóstico."
        });
    }

    /// <summary>
    /// Rejects a form submission.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [Route("Reject")]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid businessIncubatorExternalId,
        [FromRoute] Guid projectExternalId,
        [FromBody] RejectSubmissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new RejectFormSubmissionCommand
        {
            ProjectId = request.ProjectId,
            SubmissionId = request.SubmissionId,
            ReviewerUserId = userId,
            RejectionReason = request.RejectionReason
        };

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.ErrorMessages?.Select(e => e.Message) ?? ["Error al procesar la solicitud."] });
        }

        return Ok(new
        {
            success = true,
            message = "Formulario rechazado. El participante será notificado para realizar correcciones."
        });
    }
}
