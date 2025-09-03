using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

/// <summary>
/// View model for bulk user import.
/// </summary>
public class BulkImportViewModel
{
    /// <summary>
    /// Gets or sets the import file (CSV or Excel).
    /// </summary>
    [Required(ErrorMessage = "Por favor seleccione un archivo")]
    [Display(Name = "Archivo de importación")]
    public IFormFile ImportFile { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether to send welcome emails.
    /// </summary>
    [Display(Name = "Enviar correos de bienvenida")]
    public bool SendWelcomeEmails { get; set; } = true;

    /// <summary>
    /// Gets or sets the default role for imported users.
    /// </summary>
    [Display(Name = "Rol por defecto")]
    public string DefaultRole { get; set; } = "Starter";
}

/// <summary>
/// Result of a bulk import operation.
/// </summary>
public class BulkImportResult
{
    /// <summary>
    /// Gets or sets the operation ID.
    /// </summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of records processed.
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of successful imports.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed imports.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the list of errors.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Represents a user to import from CSV/Excel.
/// </summary>
public class ImportUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Identification { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
}
