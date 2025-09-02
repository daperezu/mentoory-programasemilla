namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Represents the different stages in a project lifecycle.
/// </summary>
public enum ProjectStageType
{
    /// <summary>
    /// Periodo de invitación - Initial invitation period for participants.
    /// </summary>
    Invitation = 1,

    /// <summary>
    /// Formularios iniciales - Initial form collection period.
    /// </summary>
    InitialFormCollection = 2,

    /// <summary>
    /// Mentoría - Mentoring and guidance period.
    /// </summary>
    Mentoring = 3,

    /// <summary>
    /// Formularios finales - Final form collection period.
    /// </summary>
    FinalFormCollection = 4,

    /// <summary>
    /// Cierre - Project closure and evaluation.
    /// </summary>
    Closure = 5
}