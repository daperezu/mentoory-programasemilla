using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.Submit;

/// <summary>
/// Command to submit a form for review.
/// </summary>
public sealed record SubmitFormCommand : IBaseRequest
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public long ProjectId { get; init; }

    /// <summary>
    /// Gets the submission ID.
    /// </summary>
    public long SubmissionId { get; init; }

    /// <summary>
    /// Gets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; init; }

    /// <summary>
    /// Gets the IP address of the submitter.
    /// </summary>
    public string IpAddress { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user agent of the submitter.
    /// </summary>
    public string UserAgent { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user ID of who actually submitted (for on-behalf submissions).
    /// Null if the participant submitted themselves.
    /// </summary>
    public string? SubmittedByUserId { get; init; }
}