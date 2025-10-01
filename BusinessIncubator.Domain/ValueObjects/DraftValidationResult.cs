namespace LinaSys.BusinessIncubator.Domain.ValueObjects;

/// <summary>
/// Represents the result of validating a draft against the current form schema.
/// </summary>
public class DraftValidationResult(bool isValid)
{

    /// <summary>
    /// Gets a value indicating whether gets whether the draft is valid against the current schema.
    /// </summary>
    public bool IsValid { get; } = isValid;

    /// <summary>
    /// Gets the list of required questions that are missing from the draft.
    /// </summary>
    public List<long> MissingRequiredQuestions { get; } = [];

    /// <summary>
    /// Gets the list of questions that have been removed from the current schema.
    /// </summary>
    public List<long> RemovedQuestions { get; } = [];

    /// <summary>
    /// Gets the list of questions where the answer type has changed.
    /// </summary>
    public List<(long QuestionId, string OldType, string NewType)> TypeMismatchedQuestions { get; } = [];

    /// <summary>
    /// Gets validation warnings that don't prevent submission.
    /// </summary>
    public List<string> Warnings { get; } = [];

    /// <summary>
    /// Creates a valid result.
    /// </summary>
    /// <returns></returns>
    public static DraftValidationResult Valid() => new(true);

    /// <summary>
    /// Creates an invalid result with details.
    /// </summary>
    /// <returns></returns>
    public static DraftValidationResult Invalid(
        List<long>? missingRequired = null,
        List<long>? removed = null,
        List<(long, string, string)>? typeMismatches = null,
        List<string>? warnings = null)
    {
        var result = new DraftValidationResult(false);

        if (missingRequired is not null)
        {
            result.MissingRequiredQuestions.AddRange(missingRequired);
        }

        if (removed is not null)
        {
            result.RemovedQuestions.AddRange(removed);
        }

        if (typeMismatches is not null)
        {
            result.TypeMismatchedQuestions.AddRange(typeMismatches);
        }

        if (warnings is not null)
        {
            result.Warnings.AddRange(warnings);
        }

        return result;
    }
}