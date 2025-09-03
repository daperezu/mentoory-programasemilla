using System.ComponentModel.DataAnnotations;

namespace LinaSys.BusinessIncubator.Application.Project.DTOs;

/// <summary>
/// Represents user data for registration from CSV import.
/// </summary>
public record UserRegistrationData
{
    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full name of the user.
    /// </summary>
    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "El nombre completo debe tener entre 2 y 256 caracteres.")]
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the identification number of the user.
    /// </summary>
    [Required(ErrorMessage = "El número de identificación es requerido.")]
    [RegularExpression(@"^\d{1,20}$", ErrorMessage = "El número de identificación debe contener solo números y no exceder 20 dígitos.")]
    public string IdentificationNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role for the user (optional, will use default if not provided).
    /// </summary>
    public string? Role { get; init; }
}

/// <summary>
/// Represents the result of processing a user registration.
/// </summary>
public record UserRegistrationResult
{
    /// <summary>
    /// Gets the email address that was processed.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full name that was processed.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the identification number (DUI) of the user.
    /// </summary>
    public string IdentificationNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the registration was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the error message if the registration failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user already existed.
    /// </summary>
    public bool UserAlreadyExisted { get; init; }

    /// <summary>
    /// Gets the user ID if the registration was successful.
    /// </summary>
    public string? UserId { get; init; }
}

/// <summary>
/// Represents the overall result of a batch registration operation.
/// </summary>
public record BatchRegistrationResult
{
    /// <summary>
    /// Gets the batch registration ID.
    /// </summary>
    public Guid BatchId { get; init; }

    /// <summary>
    /// Gets the total number of rows processed.
    /// </summary>
    public int TotalRows { get; init; }

    /// <summary>
    /// Gets the number of successful registrations.
    /// </summary>
    public int SuccessfulRegistrations { get; init; }

    /// <summary>
    /// Gets the number of failed registrations.
    /// </summary>
    public int FailedRegistrations { get; init; }

    /// <summary>
    /// Gets the detailed results for each user registration.
    /// </summary>
    public List<UserRegistrationResult> Results { get; init; } = [];

    /// <summary>
    /// Gets the file name that was processed.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    public string ProjectName { get; init; } = string.Empty;
}
