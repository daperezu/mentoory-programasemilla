using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    [MaxLength(256, ErrorMessage = "El correo electrónico no puede exceder 256 caracteres")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es requerida")]
    [MaxLength(50, ErrorMessage = "La identificación no puede exceder 50 caracteres")]
    [Display(Name = "Identificación")]
    public string Identification { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "La contraseña debe contener al menos: una minúscula, una mayúscula, un número y un carácter especial")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "País")]
    public string? Country { get; set; }

    [Display(Name = "Provincia")]
    public string? Province { get; set; }

    [Display(Name = "Cantón")]
    public string? Canton { get; set; }

    [Display(Name = "Distrito")]
    public string? District { get; set; }

    [Display(Name = "Dirección Completa")]
    [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? FullAddress { get; set; }

    [Display(Name = "Confirmar email automáticamente")]
    public bool EmailConfirmed { get; set; } = false;

    [Display(Name = "Generar contraseña temporal")]
    public bool GenerateTemporaryPassword { get; set; } = false;

    // Email Preferences
    [Display(Name = "Preferencias de Email")]
    public EmailPreferencesViewModel EmailPreferences { get; set; } = new();
}

public class EmailPreferencesViewModel
{
    [Display(Name = "Email de bienvenida al sistema")]
    public bool SystemWelcome { get; set; } = true;

    [Display(Name = "Email de bienvenida al proyecto")]
    public bool ProjectWelcome { get; set; } = true;

    [Display(Name = "Notificaciones de aprobación")]
    public bool Approvals { get; set; } = true;

    [Display(Name = "Notificaciones de rechazo")]
    public bool Rejections { get; set; } = true;

    [Display(Name = "Recordatorios")]
    public bool Reminders { get; set; } = true;

    [Display(Name = "Anuncios del sistema")]
    public bool Announcements { get; set; } = true;

    [Display(Name = "Asignación de tareas")]
    public bool TaskAssignments { get; set; } = true;

    [Display(Name = "Fechas límite de formularios")]
    public bool FormDeadlines { get; set; } = true;

    [Display(Name = "Mensajes de mentores")]
    public bool MentorMessages { get; set; } = true;

    [Display(Name = "Resumen diario")]
    public bool Digest { get; set; } = false;
}
