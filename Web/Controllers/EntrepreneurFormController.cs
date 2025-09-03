using System.Text.Json;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.GetOrCreateFormSubmission;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveFormDraftByExternalId;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SubmitFormByExternalId;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmissionByExternalId;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetProjectFormStructure;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Application;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models.EntrepreneurForm;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

/// <summary>
/// Controller for entrepreneur form submission workflow.
/// Redesigned to use form submission ExternalId for all operations.
/// </summary>
public class EntrepreneurFormController(ILogger<EntrepreneurFormController> logger, MediatorExecutor mediatorExecutor)
    : AuthorizedBaseController(logger, mediatorExecutor)
{
    /// <summary>
    /// Get progress endpoint - uses submission external ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("Entrepreneur/Form/{submissionExternalId:guid}/Progress")]
    public async Task<IActionResult> GetProgress(Guid submissionExternalId)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            // Get submission
            var getSubmissionQuery = new GetFormSubmissionByExternalIdQuery(submissionExternalId);
            var submissionResult = await MediatorExecutor.SendAndLogIfFailureAsync(getSubmissionQuery);

            if (!submissionResult.IsSuccess)
            {
                return Json(new { success = false, message = "Error al obtener el progreso" });
            }

            var submission = submissionResult.Value!;

            // Verify user owns this submission
            if (submission.ParticipantUserId != userId)
            {
                return Json(new { success = false, message = "No tiene permisos para ver este formulario" });
            }

            return Json(new
            {
                success = true,
                progress = submission.CompletionPercentage,
                answeredQuestions = submission.AnsweredQuestions,
                totalQuestions = submission.TotalQuestions,
                status = submission.Status,
                canEdit = submission.CanEdit,
                canSubmit = submission.CanSubmit
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting progress for submission {SubmissionId}", submissionExternalId);
            return Json(new { success = false, message = "Error al obtener el progreso" });
        }
    }

    /// <summary>
    /// Main form view - loads the form using submission external ID.
    /// </summary>
    /// <param name="submissionExternalId">The form submission external ID.</param>
    /// <returns>The form view.</returns>
    [HttpGet("Entrepreneur/Form/{submissionExternalId:guid}")]
    public async Task<IActionResult> Index(Guid submissionExternalId)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                this.SetErrorToast("Usuario no autenticado");
                return RedirectToAction("Login", "Auth");
            }

            // Get form submission by external ID
            var getSubmissionQuery = new GetFormSubmissionByExternalIdQuery(submissionExternalId);
            var submission = await MediatorExecutor.SendOrThrowAsync(getSubmissionQuery);

            // Verify the user owns this submission
            if (submission.ParticipantUserId != userId)
            {
                this.SetErrorToast("No tiene permisos para acceder a este formulario");
                return RedirectToAction("Index", "Dashboard", new { area = "Starter" });
            }

            // Check if submission can be edited
            if (!submission.CanEdit)
            {
                this.SetWarnToast($"Este formulario está en estado {submission.Status} y no puede ser editado");
                return RedirectToAction("View", new { submissionExternalId });
            }

            // Get form structure with blocks and questions
            var getStructureQuery = new GetProjectFormStructureQuery
            {
                ProjectId = submission.ProjectId,
                FormId = submission.FormId
            };

            var formStructure = await MediatorExecutor.SendOrThrowAsync(getStructureQuery);

            // TODO Get current project stage for due date
            // Note: We need to get the project's external ID first, or skip this if not critical
            // For now, setting dueDate to null as we don't have project external ID in submission
            DateTime? dueDate = null;

            // Map to view model
            var viewModel = new EntrepreneurFormViewModel
            {
                SubmissionExternalId = submission.ExternalId,
                ProjectExternalId = Guid.Empty, // No longer needed in primary flow
                ProjectName = formStructure.ProjectName,
                FormSubmissionId = submission.Id,
                Phase = submission.Phase,
                CurrentProgress = submission.CompletionPercentage,
                DueDate = dueDate,
                Status = submission.StatusEnum ?? ProjectFormSubmissionStatus.Draft,
                DraftData = submission.DraftData != null ? JsonSerializer.Serialize(submission.DraftData) : null,
                Blocks = MapToBlockViewModels(formStructure.Blocks, submission.DraftData)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading form {SubmissionId}", submissionExternalId);
            this.SetErrorToast("Error al cargar el formulario");
            return RedirectToAction("Index", "Dashboard", new { area = "Starter" });
        }
    }

    /// <summary>
    /// Save draft endpoint - uses submission external ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost("Entrepreneur/Form/{submissionExternalId:guid}/SaveDraft")]
    public async Task<IActionResult> SaveDraft(Guid submissionExternalId, [FromBody] SaveDraftRequest request)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            // Verify user owns this submission (optional - command can also check)
            var getSubmissionQuery = new GetFormSubmissionByExternalIdQuery(submissionExternalId);
            var submissionResult = await MediatorExecutor.SendAndLogIfFailureAsync(getSubmissionQuery);

            if (!submissionResult.IsSuccess || submissionResult.Value?.ParticipantUserId != userId)
            {
                return Json(new { success = false, message = "No tiene permisos para editar este formulario" });
            }

            // Parse the draft data from JSON string
            var draftData = JsonSerializer.Deserialize<DraftDataDto>(request.DraftData);
            if (draftData == null)
            {
                return Json(new { success = false, message = "Datos de borrador inválidos" });
            }

            // Save the draft
            var command = new SaveFormDraftByExternalIdCommand(
                submissionExternalId,
                draftData,
                request.AnsweredQuestions,
                request.TotalQuestions);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = "Error al guardar el borrador" });
            }

            return Json(new
            {
                success = true,
                lastSaved = result.Value!.LastSavedAt.ToString("HH:mm"),
                completionPercentage = result.Value.CompletionPercentage,
                message = "Borrador guardado exitosamente"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving draft for submission {SubmissionId}", submissionExternalId);
            return Json(new { success = false, message = "Error al guardar el borrador" });
        }
    }

    /// <summary>
    /// Entry point for form creation - redirects to the form with submission ID.
    /// Now all authorization logic is handled within the command.
    /// </summary>
    /// <param name="projectExternalId">The project external ID from invitation or link.</param>
    /// <param name="phase">The form phase (Start or Final).</param>
    /// <returns>Redirect to the form view with submission external ID.</returns>
    [HttpGet("Entrepreneur/Form/Start/{projectExternalId:guid}")]
    public async Task<IActionResult> Start(Guid projectExternalId, QuestionPhase phase = QuestionPhase.Start)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                this.SetErrorToast("Usuario no autenticado");
                return RedirectToAction("Login", "Auth");
            }

            // The command will handle all authorization logic
            var getOrCreateCommand = new GetOrCreateFormSubmissionCommand(
                projectExternalId,
                userId,
                phase);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(getOrCreateCommand);

            if (!result.IsSuccess)
            {
                // Map error codes to user-friendly messages
                var errorMessage = result.ErrorMessages?.FirstOrDefault().Message ?? "Error al acceder al formulario";
                this.SetErrorToast(errorMessage);

                // Redirect based on error type
                var errorCode = result.ErrorCode;
                if (errorCode == ResultErrorCodes.ProjectFormSubmission_AlreadySubmitted ||
                    errorCode == ResultErrorCodes.ProjectFormSubmission_AlreadyApproved)
                {
                    // If there's a submission, show its status
                    return RedirectToAction("SubmissionStatus", "Dashboard", new { area = "Starter" });
                }

                return RedirectToAction("Index", "Dashboard", new { area = "Starter" });
            }

            var submission = result.Value!;

            // Redirect to the form with the submission's external ID
            return RedirectToAction(nameof(Index), new { submissionExternalId = submission.ExternalId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting form for project {ProjectId}", projectExternalId);
            this.SetErrorToast("Error al iniciar el formulario");
            return RedirectToAction("Index", "Dashboard", new { area = "Starter" });
        }
    }

    /// <summary>
    /// Submit form endpoint - uses submission external ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost("Entrepreneur/Form/{submissionExternalId:guid}/Submit")]
    public async Task<IActionResult> Submit(Guid submissionExternalId)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            // Verify user owns this submission
            var getSubmissionQuery = new GetFormSubmissionByExternalIdQuery(submissionExternalId);
            var submissionResult = await MediatorExecutor.SendAndLogIfFailureAsync(getSubmissionQuery);

            if (!submissionResult.IsSuccess || submissionResult.Value?.ParticipantUserId != userId)
            {
                return Json(new { success = false, message = "No tiene permisos para enviar este formulario" });
            }

            // Submit the form
            var command = new SubmitFormByExternalIdCommand(
                submissionExternalId,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
            if (!result.IsSuccess)
            {
                var errorMessage = result.ErrorMessages?.FirstOrDefault().Message ?? "Error al enviar el formulario";
                return Json(new { success = false, message = errorMessage });
            }

            return Json(new
            {
                success = true,
                message = "Formulario enviado exitosamente",
                submittedAt = result.Value!.SubmittedAt,
                status = result.Value.Status,
                redirectUrl = Url.Action("Success", new { submissionExternalId })
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting form {SubmissionId}", submissionExternalId);
            return Json(new { success = false, message = "Error al enviar el formulario" });
        }
    }

    /// <summary>
    /// Success page after form submission.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("Entrepreneur/Form/{submissionExternalId:guid}/Success")]
    public async Task<IActionResult> Success(Guid submissionExternalId)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get submission to show success details
            var getSubmissionQuery = new GetFormSubmissionByExternalIdQuery(submissionExternalId);
            var submissionResult = await MediatorExecutor.SendAndLogIfFailureAsync(getSubmissionQuery);

            if (!submissionResult.IsSuccess || submissionResult.Value?.ParticipantUserId != userId)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Starter" });
            }

            var submission = submissionResult.Value;

            return View(new FormSuccessViewModel
            {
                SubmissionExternalId = submission.ExternalId,
                ProjectName = string.Empty, // Would need to load from project
                SubmittedAt = submission.SubmittedAt ?? DateTime.Now,
                Status = submission.Status,
                CompletionPercentage = submission.CompletionPercentage
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error showing success page for submission {SubmissionId}", submissionExternalId);
            return RedirectToAction("Index", "Dashboard", new { area = "Starter" });
        }
    }

    /// <summary>
    /// View-only mode for submitted/approved forms.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("Entrepreneur/Form/{submissionExternalId:guid}/View")]
    public Task<IActionResult> View(Guid submissionExternalId)
    {
        // Implementation for viewing submitted forms
        // Similar to Index but in read-only mode
        return Task.FromResult<IActionResult>(View());
    }

    private static ValidationRules? GetValidationForAnswerType(Models.EntrepreneurForm.AnswerType answerType)
    {
        return answerType switch
        {
            Models.EntrepreneurForm.AnswerType.Email => new ValidationRules
            {
                MaxLength = 254
            },
            Models.EntrepreneurForm.AnswerType.Number => new ValidationRules
            {
                MinValue = decimal.MinValue,
                MaxValue = decimal.MaxValue
            },
            Models.EntrepreneurForm.AnswerType.Text => new ValidationRules
            {
                MaxLength = 500
            },
            Models.EntrepreneurForm.AnswerType.TextArea => new ValidationRules
            {
                MaxLength = 2000
            },
            _ => null
        };
    }

    private static Models.EntrepreneurForm.AnswerType MapAnswerType(int answerType)
    {
        // Map from QuestionType enum values to AnswerType
        return answerType switch
        {
            0 => Models.EntrepreneurForm.AnswerType.Text,
            1 => Models.EntrepreneurForm.AnswerType.Number,
            2 => Models.EntrepreneurForm.AnswerType.Date,
            3 => Models.EntrepreneurForm.AnswerType.SingleChoice,
            4 => Models.EntrepreneurForm.AnswerType.MultiChoice,
            5 => Models.EntrepreneurForm.AnswerType.SingleChoice,
            6 => Models.EntrepreneurForm.AnswerType.Number,
            7 => Models.EntrepreneurForm.AnswerType.Email,
            8 => Models.EntrepreneurForm.AnswerType.Text,
            9 => Models.EntrepreneurForm.AnswerType.TextArea,
            10 => Models.EntrepreneurForm.AnswerType.SingleChoice,
            11 => Models.EntrepreneurForm.AnswerType.SingleChoice,
            _ => Models.EntrepreneurForm.AnswerType.Text
        };
    }

    // Helper methods remain the same
    private static List<FormBlockViewModel> MapToBlockViewModels(List<FormBlockDto> blocks, DraftDataDto? draftData)
    {
        var viewModels = new List<FormBlockViewModel>();

        foreach (var block in blocks)
        {
            var blockViewModel = new FormBlockViewModel
            {
                BlockId = block.Id,
                BlockName = block.Name,
                BlockDescription = null, // Description not available in domain model
                Order = block.Order,
                Questions = MapToQuestionViewModels(block.Questions, draftData),
                IsCompleted = false // Will be calculated based on answered questions
            };

            // Calculate completion based on draft data
            if (draftData?.BlockResponses != null)
            {
                var blockResponse = draftData.BlockResponses.FirstOrDefault(b => b.BlockId == block.Id);
                if (blockResponse != null)
                {
                    blockViewModel.QuestionsAnswered = blockResponse.QuestionResponses.Count(q => q.IsAnswered);
                    blockViewModel.IsCompleted = blockViewModel.QuestionsAnswered == blockViewModel.TotalQuestions;
                }
            }

            viewModels.Add(blockViewModel);
        }

        return viewModels;
    }

    private static List<QuestionViewModel> MapToQuestionViewModels(List<FormQuestionDto> questions, DraftDataDto? draftData)
    {
        return questions.Select(q =>
        {
            string? currentAnswer = null;

            // Get answer from draft data if available
            if (draftData?.BlockResponses != null)
            {
                foreach (var blockResponse in draftData.BlockResponses)
                {
                    var questionResponse = blockResponse.QuestionResponses.FirstOrDefault(qr => qr.QuestionId == q.Id);
                    if (questionResponse != null)
                    {
                        currentAnswer = questionResponse.Answer;
                        break;
                    }
                }
            }

            return new QuestionViewModel
            {
                QuestionId = q.Id,
                QuestionText = q.Text,
                HelpText = q.HelpText,
                AnswerType = MapAnswerType(q.AnswerType),
                IsRequired = q.IsRequired,
                CurrentAnswer = currentAnswer,
                Options = q.AnswerOptions?.Select(o => new AnswerOptionViewModel
                {
                    OptionId = o.Id,
                    OptionText = o.Text,
                    OptionValue = o.Value
                }).ToList() ?? new List<AnswerOptionViewModel>(),
                Validation = GetValidationForAnswerType(MapAnswerType(q.AnswerType))
            };
        }).ToList();
    }
}
