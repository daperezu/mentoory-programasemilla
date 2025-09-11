using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveCoordinatorAnswers;

/// <summary>
/// Command to save coordinator's answers for a form submission under review.
/// </summary>
public record SaveCoordinatorAnswersCommand(
    long SubmissionId,
    string CoordinatorUserId,
    DraftDataDto CoordinatorData,
    Dictionary<long, bool> PreferenceSelections) : IBaseRequest;