using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectAnswerOption;

/// <summary>
/// Command to update an answer option for a project question.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="AnswerOptionId">The answer option ID to update.</param>
/// <param name="Text">The answer option text.</param>
/// <param name="Score">The score for this answer option.</param>
/// <param name="Foda">The FODA type.</param>
/// <param name="FodaExplanation">The FODA explanation.</param>
/// <param name="Odsr">The ODSR type.</param>
/// <param name="OdsrExplanation">The ODSR explanation.</param>
/// <param name="Order">The display order.</param>
/// <param name="FollowUpQuestionText">Optional follow-up question text.</param>
public sealed record UpdateProjectAnswerOptionCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long AnswerOptionId,
    string Text,
    int Score,
    FodaType Foda,
    string FodaExplanation,
    OdsrType Odsr,
    string OdsrExplanation,
    int Order,
    string? FollowUpQuestionText = null) : IBaseRequest;