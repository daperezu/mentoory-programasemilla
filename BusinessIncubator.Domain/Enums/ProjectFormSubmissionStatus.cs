namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Represents the status of a project form submission.
/// </summary>
public enum ProjectFormSubmissionStatus
{
    /// <summary>
    /// The form is in draft state and can be edited.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// The form has been submitted and is awaiting review.
    /// </summary>
    Submitted = 2,

    /// <summary>
    /// The form has been approved.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// The form has been rejected.
    /// </summary>
    Rejected = 4
}