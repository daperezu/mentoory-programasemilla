using System.Text.Json;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.ValueObjects;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Represents a participant's form submission for a project.
/// </summary>
public class ProjectFormSubmission : Entity
{
    protected ProjectFormSubmission()
    {
    }

    private ProjectFormSubmission(
        long projectId,
        string participantUserId,
        long formId,
        int formSchemaVersion,
        QuestionPhase phase,
        long? projectStageId,
        DateTime startedAt)
    {
        ExternalId = Guid.NewGuid();
        ProjectId = projectId;
        ParticipantUserId = participantUserId;
        FormId = formId;
        FormSchemaVersion = formSchemaVersion;
        Phase = phase;
        ProjectStageId = projectStageId;
        Status = ProjectFormSubmissionStatus.Draft;
        StartedAt = startedAt;
        CompletionPercentage = 0;
        TotalQuestions = 0;
        AnsweredQuestions = 0;
    }

    /// <summary>
    /// Gets the external identifier (GUID) for public-facing operations.
    /// </summary>
    public Guid ExternalId { get; private set; }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// Gets the participant user identifier.
    /// </summary>
    public string ParticipantUserId { get; private set; }

    /// <summary>
    /// Gets the form identifier.
    /// </summary>
    public long FormId { get; private set; }

    /// <summary>
    /// Gets the form schema version at the time of submission creation.
    /// </summary>
    public int FormSchemaVersion { get; private set; }

    /// <summary>
    /// Gets the submission status.
    /// </summary>
    public ProjectFormSubmissionStatus Status { get; private set; }

    /// <summary>
    /// Gets the draft data as JSON.
    /// </summary>
    public string? DraftData { get; private set; }

    /// <summary>
    /// Gets when the form was started.
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Gets when the form was submitted.
    /// </summary>
    public DateTime? SubmittedAt { get; private set; }

    /// <summary>
    /// Gets when the form was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>
    /// Gets the user who approved the submission.
    /// </summary>
    public string? ApprovedByUserId { get; private set; }

    /// <summary>
    /// Gets when the form was rejected.
    /// </summary>
    public DateTime? RejectedAt { get; private set; }

    /// <summary>
    /// Gets the rejection reason if the form was rejected.
    /// </summary>
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Gets the question phase this submission belongs to.
    /// </summary>
    public QuestionPhase Phase { get; private set; }

    /// <summary>
    /// Gets the project stage ID associated with this submission.
    /// </summary>
    public long? ProjectStageId { get; private set; }

    /// <summary>
    /// Gets the completion percentage of the form.
    /// </summary>
    public int CompletionPercentage { get; private set; }

    /// <summary>
    /// Gets the last auto-save timestamp.
    /// </summary>
    public DateTime? LastAutoSaveAt { get; private set; }

    /// <summary>
    /// Gets the total number of questions in the form.
    /// </summary>
    public int TotalQuestions { get; private set; }

    /// <summary>
    /// Gets the number of answered questions.
    /// </summary>
    public int AnsweredQuestions { get; private set; }

    /// <summary>
    /// Navigation property for EF Core.
    /// </summary>
    internal virtual Project Project { get; private set; } = null!;

    /// <summary>
    /// Navigation property for the project stage.
    /// </summary>
    internal virtual ProjectStage? ProjectStage { get; private set; }

    /// <summary>
    /// Gets the appropriate phase for a given stage type.
    /// </summary>
    /// <param name="stageType">The stage type.</param>
    /// <returns>The corresponding question phase.</returns>
    public static QuestionPhase GetPhaseForStage(ProjectStageType stageType)
    {
        return stageType switch
        {
            ProjectStageType.InitialFormCollection => QuestionPhase.Start,
            ProjectStageType.FinalFormCollection => QuestionPhase.Final,
            _ => QuestionPhase.Undefined
        };
    }

    /// <summary>
    /// Creates a new form submission for a participant with phase support.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="participantUserId">The participant user ID.</param>
    /// <param name="formId">The form ID.</param>
    /// <param name="formSchemaVersion">The current form schema version.</param>
    /// <param name="phase">The question phase.</param>
    /// <param name="projectStageId">The project stage ID (optional).</param>
    /// <param name="startedAt">The start timestamp.</param>
    /// <returns>A new form submission instance.</returns>
    public static ProjectFormSubmission CreateForPhase(
        long projectId,
        string participantUserId,
        long formId,
        int formSchemaVersion,
        QuestionPhase phase,
        long? projectStageId,
        DateTime startedAt)
    {
        if (phase == QuestionPhase.None)
        {
            throw new ArgumentException("La fase no puede ser None.", nameof(phase));
        }

        return new ProjectFormSubmission(
            projectId,
            participantUserId,
            formId,
            formSchemaVersion,
            phase,
            projectStageId,
            startedAt);
    }

    /// <summary>
    /// Saves a draft of the form data with progress tracking.
    /// </summary>
    /// <param name="draftData">The form data as JSON.</param>
    /// <param name="answeredQuestions">The number of questions answered.</param>
    /// <param name="totalQuestions">The total number of questions.</param>
    /// <param name="autoSaveTime">The auto-save timestamp.</param>
    public void SaveDraft(string draftData, int answeredQuestions, int totalQuestions, DateTime autoSaveTime)
    {
        if (Status != ProjectFormSubmissionStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden guardar borradores cuando el formulario está en estado borrador.");
        }

        if (answeredQuestions < 0)
        {
            throw new ArgumentException("El número de preguntas respondidas no puede ser negativo.", nameof(answeredQuestions));
        }

        if (totalQuestions <= 0)
        {
            throw new ArgumentException("El número total de preguntas debe ser mayor que cero.", nameof(totalQuestions));
        }

        if (answeredQuestions > totalQuestions)
        {
            throw new ArgumentException("El número de preguntas respondidas no puede ser mayor que el total.", nameof(answeredQuestions));
        }

        DraftData = draftData;
        AnsweredQuestions = answeredQuestions;
        TotalQuestions = totalQuestions;
        CompletionPercentage = totalQuestions > 0 ? (answeredQuestions * 100) / totalQuestions : 0;
        LastAutoSaveAt = autoSaveTime;
    }

    /// <summary>
    /// Submits the form for review.
    /// </summary>
    /// <param name="submittedAt">The submission timestamp.</param>
    public void Submit(DateTime submittedAt)
    {
        if (Status != ProjectFormSubmissionStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden enviar formularios en estado borrador.");
        }

        if (string.IsNullOrWhiteSpace(DraftData))
        {
            throw new InvalidOperationException("No se puede enviar un formulario sin datos.");
        }

        Status = ProjectFormSubmissionStatus.Submitted;
        SubmittedAt = submittedAt;
    }

    /// <summary>
    /// Approves the form submission.
    /// </summary>
    /// <param name="approverUserId">The user ID of the approver.</param>
    /// <param name="approvedAt">The approval timestamp.</param>
    public void Approve(string approverUserId, DateTime approvedAt)
    {
        if (Status != ProjectFormSubmissionStatus.Submitted)
        {
            throw new InvalidOperationException("Solo se pueden aprobar formularios enviados.");
        }

        Status = ProjectFormSubmissionStatus.Approved;
        ApprovedAt = approvedAt;
        ApprovedByUserId = approverUserId;
    }

    /// <summary>
    /// Rejects the form submission.
    /// </summary>
    /// <param name="reason">The reason for rejection.</param>
    /// <param name="rejectedAt">The rejection timestamp.</param>
    public void Reject(string reason, DateTime rejectedAt)
    {
        if (Status != ProjectFormSubmissionStatus.Submitted)
        {
            throw new InvalidOperationException("Solo se pueden rechazar formularios enviados.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Se debe proporcionar una razón para el rechazo.", nameof(reason));
        }

        Status = ProjectFormSubmissionStatus.Rejected;
        RejectionReason = reason;
        RejectedAt = rejectedAt;
    }

    /// <summary>
    /// Allows editing a rejected form.
    /// </summary>
    public void EnableEditingAfterRejection()
    {
        if (Status != ProjectFormSubmissionStatus.Rejected)
        {
            throw new InvalidOperationException("Solo se pueden editar formularios rechazados.");
        }

        Status = ProjectFormSubmissionStatus.Draft;
        RejectionReason = null;
    }

    /// <summary>
    /// Updates the form schema version when the draft is migrated.
    /// </summary>
    /// <param name="newVersion">The new schema version.</param>
    public void UpdateFormSchemaVersion(int newVersion)
    {
        if (newVersion <= FormSchemaVersion)
        {
            throw new ArgumentException("La nueva versión debe ser mayor que la versión actual.", nameof(newVersion));
        }

        FormSchemaVersion = newVersion;
    }

    /// <summary>
    /// Validates the draft data against the current form structure.
    /// </summary>
    /// <param name="currentStructure">The current project knowledge structure.</param>
    /// <returns>A validation result indicating whether the draft is valid.</returns>
    public DraftValidationResult ValidateDraftAgainstSchema(ProjectKnowledgeStructure currentStructure)
    {
        if (string.IsNullOrWhiteSpace(DraftData))
        {
            return DraftValidationResult.Invalid(warnings: ["El borrador está vacío."]);
        }

        // Check if schema version matches
        if (FormSchemaVersion == currentStructure.CurrentVersion)
        {
            return DraftValidationResult.Valid();
        }

        try
        {
            // Parse draft JSON as dynamic to avoid DTO dependency in domain
            using var document = JsonDocument.Parse(DraftData);
            var root = document.RootElement;

            // Build question maps
            var currentQuestions = new Dictionary<long, ProjectQuestion>();
            foreach (var module in currentStructure.ProjectModules)
            {
                foreach (var topic in module.ProjectTopics)
                {
                    foreach (var question in topic.ProjectQuestions)
                    {
                        currentQuestions[question.Id] = question;
                    }
                }
            }

            // Extract question IDs from draft
            var draftQuestionIds = new HashSet<long>();
            var draftQuestionTypes = new Dictionary<long, int>();

            if (root.TryGetProperty("blockResponses", out var blockResponses) && blockResponses.ValueKind == JsonValueKind.Array)
            {
                foreach (var block in blockResponses.EnumerateArray())
                {
                    if (block.TryGetProperty("questionResponses", out var questionResponses) && questionResponses.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var question in questionResponses.EnumerateArray())
                        {
                            if (question.TryGetProperty("questionId", out var questionId) && questionId.TryGetInt64(out var id))
                            {
                                draftQuestionIds.Add(id);

                                if (question.TryGetProperty("answerType", out var answerType) && answerType.TryGetInt32(out var type))
                                {
                                    draftQuestionTypes[id] = type;
                                }
                            }
                        }
                    }
                }
            }

            // Find missing required questions (all questions are considered required)
            var missingRequired = currentQuestions
                .Where(q => !draftQuestionIds.Contains(q.Key))
                .Select(q => q.Key)
                .ToList();

            // Find removed questions
            var removed = draftQuestionIds
                .Where(id => !currentQuestions.ContainsKey(id))
                .ToList();

            // Check type mismatches
            var typeMismatches = new List<(long QuestionId, string OldType, string NewType)>();
            foreach (var (questionId, draftType) in draftQuestionTypes)
            {
                if (currentQuestions.TryGetValue(questionId, out var current))
                {
                    if (draftType != (int)current.AnswerType)
                    {
                        typeMismatches.Add((
                            questionId,
                            ((AnswerType)draftType).ToString(),
                            current.AnswerType.ToString()));
                    }
                }
            }

            // Build warnings
            var warnings = new List<string>
            {
                $"La versión del formulario ({FormSchemaVersion}) no coincide con la versión actual ({currentStructure.CurrentVersion})."
            };

            if (missingRequired.Any() || removed.Any() || typeMismatches.Any())
            {
                return DraftValidationResult.Invalid(
                    missingRequired: missingRequired,
                    removed: removed,
                    typeMismatches: typeMismatches,
                    warnings: warnings);
            }

            // Version mismatch but no structural issues
            return DraftValidationResult.Invalid(warnings: warnings);
        }
        catch (JsonException)
        {
            return DraftValidationResult.Invalid(
                warnings: ["El formato del borrador no es válido."]);
        }
    }

    /// <summary>
    /// Checks if the submission is within the submission window based on the stage.
    /// </summary>
    /// <param name="stage">The project stage to check against.</param>
    /// <param name="currentDate">The current date to check against.</param>
    /// <returns>True if within the submission window, false otherwise.</returns>
    public bool IsWithinSubmissionWindow(ProjectStage? stage, DateTime currentDate)
    {
        if (stage is null)
        {
            return false;
        }

        return stage.IsWithinPeriod(currentDate) && stage.IsActive;
    }

    /// <summary>
    /// Creates a new form submission for a participant (legacy method for backward compatibility).
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="participantUserId">The participant user ID.</param>
    /// <param name="formId">The form ID.</param>
    /// <param name="formSchemaVersion">The current form schema version.</param>
    /// <param name="startedAt">The start timestamp.</param>
    /// <returns>A new form submission instance.</returns>
    internal static ProjectFormSubmission Create(
        long projectId,
        string participantUserId,
        long formId,
        int formSchemaVersion,
        DateTime startedAt)
    {
        // Default to Start phase for backward compatibility
        return new ProjectFormSubmission(
            projectId,
            participantUserId,
            formId,
            formSchemaVersion,
            QuestionPhase.Start,
            null,
            startedAt);
    }
}