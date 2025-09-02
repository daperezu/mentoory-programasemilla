using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;
using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmission;

/// <summary>
/// DTO for form submission details.
/// </summary>
public class FormSubmissionDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the external ID for public-facing operations.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the form ID.
    /// </summary>
    public long FormId { get; set; }

    /// <summary>
    /// Gets or sets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets when the form was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the form was submitted.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets when the form was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Gets or sets the rejection reason.
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Gets or sets the draft data.
    /// </summary>
    public DraftDataDto? DraftData { get; set; }

    /// <summary>
    /// Gets or sets whether the user can edit the form.
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets whether the user can submit the form.
    /// </summary>
    public bool CanSubmit { get; set; }

    /// <summary>
    /// Gets or sets the form schema version.
    /// </summary>
    public int FormSchemaVersion { get; set; }

    /// <summary>
    /// Gets or sets the status as enum.
    /// </summary>
    public ProjectFormSubmissionStatus? StatusEnum { get; set; }

    /// <summary>
    /// Gets or sets the user who approved the submission.
    /// </summary>
    public string? ApprovedByUserId { get; set; }

    /// <summary>
    /// Gets or sets when the form was rejected.
    /// </summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>
    /// Gets or sets the question phase.
    /// </summary>
    public QuestionPhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the project stage ID.
    /// </summary>
    public long? ProjectStageId { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the last auto-save timestamp.
    /// </summary>
    public DateTime? LastAutoSaveAt { get; set; }

    /// <summary>
    /// Gets or sets the total number of questions.
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Gets or sets the number of answered questions.
    /// </summary>
    public int AnsweredQuestions { get; set; }
}