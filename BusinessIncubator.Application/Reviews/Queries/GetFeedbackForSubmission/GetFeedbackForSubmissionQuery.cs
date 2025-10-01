using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Queries.GetFeedbackForSubmission;

/// <summary>
/// Query to get feedback conversations for a submission.
/// </summary>
public record GetFeedbackForSubmissionQuery(
    long SubmissionId,
    string UserId) : IBaseRequest<List<FeedbackConversationDto>>;

/// <summary>
/// DTO for feedback conversation.
/// </summary>
public class FeedbackConversationDto
{
    /// <summary>
    /// Gets or sets the feedback ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the block ID.
    /// </summary>
    public long? BlockId { get; set; }

    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    public long? QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string? QuestionText { get; set; }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    public string FeedbackText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback type.
    /// </summary>
    public FeedbackType Type { get; set; }

    /// <summary>
    /// Gets or sets the feedback status.
    /// </summary>
    public FeedbackStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the feedback is from a participant.
    /// </summary>
    public bool IsFromParticipant { get; set; }

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the replies.
    /// </summary>
    public List<FeedbackReplyDto> Replies { get; set; } = new();
}

/// <summary>
/// DTO for feedback reply.
/// </summary>
public class FeedbackReplyDto
{
    /// <summary>
    /// Gets or sets the reply ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    public string FeedbackText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the reply is from a participant.
    /// </summary>
    public bool IsFromParticipant { get; set; }

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Handler for GetFeedbackForSubmissionQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetFeedbackForSubmissionQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="logger">The logger.</param>
public class GetFeedbackForSubmissionQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetFeedbackForSubmissionQueryHandler> logger) : BaseCommandHandler<GetFeedbackForSubmissionQuery, List<FeedbackConversationDto>>
{
    /// <inheritdoc/>
    public override async Task<Result<List<FeedbackConversationDto>>> Handle(
        GetFeedbackForSubmissionQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the submission to verify access
            var submission = await repository.GetFormSubmissionByIdAsync(request.SubmissionId, cancellationToken);
            if (submission is null)
            {
                return Failure(
                    ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(GetFeedbackForSubmissionQuery), "No se encontró la submisión."));
            }

            // Verify user has access (either participant or coordinator)
            var isParticipant = submission.ParticipantUserId == request.UserId;
            var isCoordinator = await repository.IsUserProjectParticipantAsync(submission.ProjectId, request.UserId, cancellationToken);

            if (!isParticipant && !isCoordinator)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(GetFeedbackForSubmissionQuery), "No tiene permisos para ver el feedback de esta submisión."));
            }

            // Get all feedback with replies for this submission
            var allFeedback = await repository.GetFeedbackWithRepliesForSubmissionAsync(request.SubmissionId, cancellationToken);

            // Group feedback into conversations (parent feedback + replies)
            var conversations = new List<FeedbackConversationDto>();
            var parentFeedback = allFeedback.Where(f => !f.ParentFeedbackId.HasValue).ToList();

            foreach (var parent in parentFeedback)
            {
                // Get author name
                var authorName = await GetUserDisplayName(parent.CreatedBy, cancellationToken);

                // Get block/question details
                string blockName = parent.BlockId.HasValue ? $"Bloque {parent.BlockId}" : "General";
                string? questionText = parent.QuestionId.HasValue ? $"Pregunta {parent.QuestionId}" : null;

                // Build conversation DTO
                var conversation = new FeedbackConversationDto
                {
                    Id = parent.Id,
                    BlockId = parent.BlockId,
                    QuestionId = parent.QuestionId,
                    BlockName = blockName,
                    QuestionText = questionText,
                    FeedbackText = parent.FeedbackText,
                    Type = parent.FeedbackType,
                    Status = parent.Status,
                    AuthorName = authorName,
                    IsFromParticipant = parent.IsFromParticipant,
                    CreatedAt = parent.CreatedAt,
                    Replies = new List<FeedbackReplyDto>()
                };

                // Add replies
                var replies = allFeedback.Where(f => f.ParentFeedbackId == parent.Id)
                    .OrderBy(f => f.CreatedAt)
                    .ToList();

                foreach (var reply in replies)
                {
                    var replyAuthorName = await GetUserDisplayName(reply.CreatedBy, cancellationToken);
                    conversation.Replies.Add(new FeedbackReplyDto
                    {
                        Id = reply.Id,
                        FeedbackText = reply.FeedbackText,
                        AuthorName = replyAuthorName,
                        IsFromParticipant = reply.IsFromParticipant,
                        CreatedAt = reply.CreatedAt
                    });
                }

                conversations.Add(conversation);
            }

            logger.LogInformation(
                "Retrieved {ConversationCount} feedback conversations for submission {SubmissionId}",
                conversations.Count,
                request.SubmissionId);

            return Success(conversations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving feedback for submission {SubmissionId}", request.SubmissionId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetFeedbackForSubmissionQuery), "Ocurrió un error al obtener el feedback."));
        }
    }

    private Task<string> GetUserDisplayName(string userId, CancellationToken cancellationToken)
    {
        // For now, return a simple display name based on user ID
        // This can be enhanced later when proper user lookup is available
        return Task.FromResult($"Usuario {userId.Substring(0, Math.Min(8, userId.Length))}");
    }
}
