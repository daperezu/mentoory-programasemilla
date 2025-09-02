using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;

/// <summary>
/// Command to save a draft of a form submission.
/// </summary>
public sealed record SaveDraftCommand : IBaseRequest
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
    public string ParticipantUserId { get; init; }

    /// <summary>
    /// Gets the draft data to save.
    /// </summary>
    public DraftDataDto DraftData { get; init; } = null!;
}