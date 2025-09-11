namespace LinaSys.BusinessIncubator.Application.IntegrationEvents;

/// <summary>
/// DTO representing both starter and coordinator answers for a question.
/// </summary>
public sealed record DualAnswerDto(
    long BlockId,
    string BlockName,
    long QuestionId,
    string QuestionText,
    AnswerDataDto? StarterAnswer,
    AnswerDataDto? CoordinatorAnswer,
    bool UseCoordinatorForDiagnosis);

/// <summary>
/// DTO representing answer data from either starter or coordinator.
/// </summary>
public sealed record AnswerDataDto(
    long? AnswerOptionId,
    string AnswerText,
    string? UserInput,
    string? FollowUpAnswer,
    int Score,
    LinaSys.BusinessIncubator.Domain.Enums.FodaType? Foda,
    LinaSys.BusinessIncubator.Domain.Enums.OdsrType? Odsr);