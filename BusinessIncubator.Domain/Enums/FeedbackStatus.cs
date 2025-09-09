namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Feedback status enumeration.
/// </summary>
public enum FeedbackStatus
{
    /// <summary>
    /// Feedback requires review.
    /// </summary>
    ReviewNeeded = 0,

    /// <summary>
    /// Feedback has been closed.
    /// </summary>
    ReviewClosed = 1
}