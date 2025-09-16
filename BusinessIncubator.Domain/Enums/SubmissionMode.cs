namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Represents the mode of form submission.
/// </summary>
public enum SubmissionMode
{
    /// <summary>
    /// Form was submitted by the participant themselves.
    /// </summary>
    Self = 1,

    /// <summary>
    /// Form was submitted on behalf of the participant by a coordinator or administrator.
    /// </summary>
    OnBehalf = 2
}