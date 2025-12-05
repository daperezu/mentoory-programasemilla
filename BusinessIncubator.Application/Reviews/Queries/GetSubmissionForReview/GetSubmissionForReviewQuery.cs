using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Queries.GetSubmissionForReview;

/// <summary>
/// Query to get a submission for review with all details.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetSubmissionForReviewQuery(
    long SubmissionId,
    Guid ProjectId) : IBaseRequest<SubmissionReviewDto>;

/// <summary>
/// DTO for submission review details.
/// </summary>
public class SubmissionReviewDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form version.
    /// </summary>
    public int FormVersion { get; set; }

    /// <summary>
    /// Gets or sets the submission status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission date.
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the blocks.
    /// </summary>
    public List<BlockReviewDto> Blocks { get; set; } = [];

    /// <summary>
    /// Gets or sets existing reviews.
    /// </summary>
    public List<ReviewHistoryDto> ReviewHistory { get; set; } = [];

    /// <summary>
    /// Gets or sets the current review status.
    /// </summary>
    public string? CurrentReviewStatus { get; set; }

    /// <summary>
    /// Gets or sets the current review comments.
    /// </summary>
    public string? CurrentReviewComments { get; set; }

    /// <summary>
    /// Gets or sets the current review deadline.
    /// </summary>
    public DateTime? CurrentReviewDeadline { get; set; }
}

/// <summary>
/// DTO for block review information.
/// </summary>
public class BlockReviewDto
{
    /// <summary>
    /// Gets or sets the block ID.
    /// </summary>
    public long BlockId { get; set; }

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block description.
    /// </summary>
    public string? BlockDescription { get; set; }

    /// <summary>
    /// Gets or sets the block order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the questions.
    /// </summary>
    public List<QuestionReviewDto> Questions { get; set; } = [];

    /// <summary>
    /// Gets or sets the feedback items for this block.
    /// </summary>
    public List<FeedbackItemDto> Feedback { get; set; } = [];
}

/// <summary>
/// DTO for question review information.
/// </summary>
public class QuestionReviewDto
{
    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    public long QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the question type.
    /// </summary>
    public string QuestionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the answer type as integer for JavaScript compatibility.
    /// </summary>
    public int AnswerType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the question is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the user's answer.
    /// </summary>
    public string? Answer { get; set; }

    /// <summary>
    /// Gets or sets the answer options for choice questions.
    /// </summary>
    public List<AnswerOptionDto> AnswerOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the feedback items for this question.
    /// </summary>
    public List<FeedbackItemDto> Feedback { get; set; } = [];

    /// <summary>
    /// Gets or sets module information for tracking.
    /// </summary>
    public ProjectFormSubmissions.Commands.SaveDraft.ModuleInfoDto? ModuleInfo { get; set; }

    /// <summary>
    /// Gets or sets topic information for tracking.
    /// </summary>
    public ProjectFormSubmissions.Commands.SaveDraft.TopicInfoDto? TopicInfo { get; set; }
}

/// <summary>
/// DTO for answer options.
/// </summary>
public class AnswerOptionDto
{
    /// <summary>
    /// Gets or sets the answer option ID.
    /// </summary>
    public long AnswerOptionId { get; set; }

    /// <summary>
    /// Gets or sets the answer option text.
    /// </summary>
    public string AnswerOptionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score for this option.
    /// </summary>
    public int Score { get; set; }
}

/// <summary>
/// DTO for feedback items.
/// </summary>
public class FeedbackItemDto
{
    /// <summary>
    /// Gets or sets the feedback ID.
    /// </summary>
    public long FeedbackId { get; set; }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    public string FeedbackText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback type.
    /// </summary>
    public string FeedbackType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reviewer name.
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for review history.
/// </summary>
public class ReviewHistoryDto
{
    /// <summary>
    /// Gets or sets the review ID.
    /// </summary>
    public long ReviewId { get; set; }

    /// <summary>
    /// Gets or sets the reviewer name.
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the review status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the review date.
    /// </summary>
    public DateTime ReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the general comments.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets or sets the feedback count.
    /// </summary>
    public int FeedbackCount { get; set; }
}

/// <summary>
/// Handler for GetSubmissionForReviewQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetSubmissionForReviewQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="logger">The logger.</param>
public class GetSubmissionForReviewQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetSubmissionForReviewQueryHandler> logger) : BaseCommandHandler<GetSubmissionForReviewQuery, SubmissionReviewDto>
{

    /// <inheritdoc/>
    public override async Task<Result<SubmissionReviewDto>> Handle(
        GetSubmissionForReviewQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get submission with all related data
            var submission = await repository.GetSubmissionWithDetailsForReviewAsync(
                request.SubmissionId,
                cancellationToken);

            if (submission is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(GetSubmissionForReviewQuery), $"La solicitud con ID {request.SubmissionId} no fue encontrada."));
            }

            // Load project with knowledge structure to get question metadata
            var project = await repository.GetProjectWithKnowledgeStructureByIdAsync(
                submission.ProjectId,
                cancellationToken);

            if (project is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(GetSubmissionForReviewQuery), $"El proyecto no fue encontrado."));
            }

            // Build a map of questions with their answer options
            var questionMap = new Dictionary<long, (int AnswerType, bool IsRequired, List<AnswerOptionDto> Options)>();
            if (project.HasKnowledgeStructure())
            {
                var knowledgeStructure = project.GetKnowledgeStructure();
                if (knowledgeStructure is not null)
                {
                    foreach (var block in project.ProjectBlocks)
                    {
                        foreach (var question in block.ProjectQuestions)
                        {
                            var options = question.ProjectAnswerOptions?.Select(ao => new AnswerOptionDto
                            {
                                AnswerOptionId = ao.Id,
                                AnswerOptionText = ao.Text,
                                Score = ao.Score
                            }).ToList() ?? [];

                            questionMap[question.Id] = ((int)question.AnswerType, question.IsRequired, options);
                        }
                    }
                }
            }

            // Authorization is now handled by the RequiresPermission attribute and AuthorizationBehavior
            // The ProjectId in the query will be used to verify access via ProtectedResource

            // Map to DTO
            var dto = new SubmissionReviewDto
            {
                SubmissionId = submission.Id,
                ExternalId = Guid.NewGuid(), // TODO: Add ExternalId to ProjectFormSubmission if needed
                UserId = submission.ParticipantUserId,
                UserName = "Usuario", // TODO: Get user name from UserManager if needed
                UserEmail = string.Empty, // TODO: Get user email from UserManager if needed
                FormName = "Formulario", // TODO: Get form name from ProjectForm if needed
                FormVersion = submission.FormSchemaVersion,
                Status = submission.Status.ToString(),
                SubmittedAt = submission.SubmittedAt ?? submission.StartedAt,
                CompletionPercentage = 0 // TODO: Calculate completion percentage if needed
            };

            // Map blocks and questions from draft data
            if (!string.IsNullOrEmpty(submission.DraftData))
            {
                try
                {
                    var draftData = System.Text.Json.JsonSerializer.Deserialize<ProjectFormSubmissions.Commands.SaveDraft.DraftDataDto>(
                        submission.DraftData,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (draftData?.BlockResponses != null)
                    {
                        dto.Blocks = draftData.BlockResponses.Select(block => new BlockReviewDto
                        {
                            BlockId = block.BlockId,
                            BlockName = block.BlockName,
                            BlockDescription = null, // Description not stored in draft
                            Order = 0, // Order not stored in draft
                            Questions = block.QuestionResponses?.Select(q =>
                            {
                                // Get metadata from knowledge structure
                                var hasMetadata = questionMap.TryGetValue(q.QuestionId, out var metadata);

                                return new QuestionReviewDto
                                {
                                    QuestionId = q.QuestionId,
                                    QuestionText = q.QuestionText,
                                    QuestionType = MapAnswerTypeToString(q.AnswerType),
                                    AnswerType = hasMetadata ? metadata.AnswerType : q.AnswerType,
                                    IsRequired = hasMetadata ? metadata.IsRequired : true,
                                    Answer = q.Answer,
                                    AnswerOptions = hasMetadata ? metadata.Options : [],
                                    Feedback = [], // No feedback initially
                                    ModuleInfo = q.ModuleInfo,
                                    TopicInfo = q.TopicInfo
                                };
                            }).ToList() ?? [],
                            Feedback = [] // No feedback initially
                        }).ToList();

                        // Calculate completion percentage
                        var totalQuestions = dto.Blocks.Sum(b => b.Questions.Count);
                        var answeredQuestions = dto.Blocks.Sum(b => b.Questions.Count(q => !string.IsNullOrEmpty(q.Answer)));
                        dto.CompletionPercentage = totalQuestions > 0 ? (answeredQuestions * 100) / totalQuestions : 0;
                    }
                    else
                    {
                        dto.Blocks = [];
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error deserializing draft data for submission {SubmissionId}", submission.Id);
                    dto.Blocks = [];
                }
            }
            else
            {
                dto.Blocks = [];
            }

            // Get review history
            var reviews = await repository.GetReviewsBySubmissionIdAsync(
                request.SubmissionId,
                cancellationToken);

            dto.ReviewHistory = reviews.Select(r => new ReviewHistoryDto
            {
                ReviewId = r.Id,
                ReviewerName = r.ReviewerId, // TODO: Get reviewer name from UserManager if needed
                Status = r.Status.ToString(),
                ReviewedAt = r.ReviewedAt,
                Comments = r.GeneralComments,
                FeedbackCount = r.FeedbackItems.Count
            }).OrderByDescending(r => r.ReviewedAt).ToList();

            // Get current review status
            var currentReview = reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
            if (currentReview is not null)
            {
                dto.CurrentReviewStatus = currentReview.Status.ToString();
                dto.CurrentReviewComments = currentReview.GeneralComments;
                dto.CurrentReviewDeadline = currentReview.NewDeadline;

                // TODO: Map feedback to blocks and questions if needed
            }

            logger.LogInformation(
                "Retrieved submission {SubmissionId} for review",
                request.SubmissionId);

            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving submission {SubmissionId} for review", request.SubmissionId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetSubmissionForReviewQuery), "Ocurrió un error al obtener la solicitud para revisión."));
        }
    }

    private static string MapAnswerTypeToString(int answerType)
    {
        return answerType switch
        {
            1 => "SingleChoice",
            2 => "MultiChoice",
            3 => "FreeText",
            4 => "Numeric",
            5 => "Date",
            6 => "PersonId",
            7 => "IdType",
            8 => "Gender",
            9 => "MaritalStatus",
            10 => "Email",
            11 => "PhoneNumber",
            12 => "Nationality",
            _ => "Text"
        };
    }
}
