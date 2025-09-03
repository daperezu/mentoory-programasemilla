namespace LinaSys.Web.Models.EntrepreneurForm;

/// <summary>
/// Request model for validating a form wizard step.
/// </summary>
public class ValidateStepRequest
{
    /// <summary>
    /// Gets or sets the step number being validated.
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Gets or sets the answers provided in the step, keyed by question ID.
    /// </summary>
    public Dictionary<long, string>? Answers { get; set; }

    /// <summary>
    /// Gets or sets list of required question IDs in this step.
    /// </summary>
    public List<long>? RequiredQuestions { get; set; }

    /// <summary>
    /// Gets or sets list of question IDs that should contain email addresses.
    /// </summary>
    public List<long>? EmailQuestions { get; set; }

    /// <summary>
    /// Gets or sets list of question IDs that should contain numeric values.
    /// </summary>
    public List<long>? NumericQuestions { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage of the current step.
    /// </summary>
    public int CompletionPercentage { get; set; }
}
