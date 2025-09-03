using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.StartFormSubmission;

/// <summary>
/// Command to start a new form submission for a participant.
/// </summary>
public sealed record StartFormSubmissionCommand : IBaseRequest<long>
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public long ProjectId { get; init; }

    /// <summary>
    /// Gets the form ID.
    /// </summary>
    public long FormId { get; init; }

    /// <summary>
    /// Gets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; init; } = string.Empty;
}