using System.ComponentModel.DataAnnotations;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Attributes;

namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

public class CreateUserViewModel : IValidatableObject
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

    // Role and Access Assignment Fields
    [Required(ErrorMessage = "El rol es requerido")]
    [Display(Name = "Rol del Usuario")]
    public string SelectedRole { get; set; } = string.Empty;

    [Display(Name = "Incubadora")]
    [RequiredIfRole("Coordinator,Administrator,Liaison,Mentor,Guide,Facilitator",
        ErrorMessage = "La incubadora es requerida para el rol seleccionado")]
    public long? SelectedIncubatorId { get; set; }

    [Display(Name = "Proyecto")]
    [RequiredIfRole("Starter,Coordinator,Mentor,Guide,Facilitator",
        ErrorMessage = "El proyecto es requerido para el rol seleccionado")]
    public long? SelectedProjectId { get; set; }

    // Location Fields
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

    // Account Settings
    [Display(Name = "Confirmar email automáticamente")]
    public bool EmailConfirmed { get; set; } = false;

    [Display(Name = "Generar contraseña temporal")]
    public bool GenerateTemporaryPassword { get; set; } = false;

    // Email Preferences
    [Display(Name = "Preferencias de Email")]
    public EmailPreferencesViewModel EmailPreferences { get; set; } = new();

    // Collections for dropdowns
    public List<RoleSelectItem> AvailableRoles { get; set; } = [];
    public List<IncubatorSelectItem> AvailableIncubators { get; set; } = [];
    public List<ProjectSelectItem> AvailableProjects { get; set; } = [];

    // Custom validation
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Password validation when not generating temporary password
        if (!GenerateTemporaryPassword)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("La contraseña es requerida", [nameof(Password)]);
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                yield return new ValidationResult("La confirmación de contraseña es requerida", [nameof(ConfirmPassword)]);
            }
        }

        // Role-based validation for incubator and project
        if (!string.IsNullOrEmpty(SelectedRole))
        {
            switch (SelectedRole)
            {
                case Roles.GlobalAdministrator:
                    // Global Administrator doesn't need incubator or project
                    break;

                case Roles.Administrator:
                case Roles.Liaison:
                    // Administrator and Liaison need incubator only
                    if (SelectedIncubatorId is not > 0)
                    {
                        yield return new ValidationResult(
                            $"La incubadora es requerida para el rol {GetRoleDisplayName(SelectedRole)}",
                            [nameof(SelectedIncubatorId)]);
                    }

                    break;

                case Roles.Coordinator:
                    // Coordinator requires both incubator and project
                    if (SelectedIncubatorId is not > 0)
                    {
                        yield return new ValidationResult(
                            "La incubadora es requerida para el rol Coordinador",
                            [nameof(SelectedIncubatorId)]);
                    }

                    if (SelectedProjectId is not > 0)
                    {
                        yield return new ValidationResult(
                            "El proyecto es requerido para el rol Coordinador",
                            [nameof(SelectedProjectId)]);
                    }

                    break;

                case Roles.Starter:
                case Roles.Mentor:
                case Roles.Guide:
                case Roles.Facilitator:
                    // These roles need both incubator and project
                    if (SelectedIncubatorId is not > 0)
                    {
                        yield return new ValidationResult(
                            $"La incubadora es requerida para el rol {GetRoleDisplayName(SelectedRole)}",
                            [nameof(SelectedIncubatorId)]);
                    }

                    if (SelectedProjectId is not > 0)
                    {
                        yield return new ValidationResult(
                            $"El proyecto es requerido para el rol {GetRoleDisplayName(SelectedRole)}",
                            [nameof(SelectedProjectId)]);
                    }

                    break;
            }
        }
    }

    private string GetRoleDisplayName(string role)
    {
        return role switch
        {
            "Starter" => "Emprendedor",
            "Coordinator" => "Coordinador",
            "Mentor" => "Mentor",
            "Guide" => "Guía",
            "Facilitator" => "Facilitador",
            "Liaison" => "Enlace",
            "Administrator" => "Administrador",
            "GlobalAdministrator" => "Administrador Global",
            _ => role
        };
    }
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

// Select list item classes
public class RoleSelectItem
{
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresIncubator { get; set; }
    public bool RequiresProject { get; set; }
}

public class IncubatorSelectItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

public class ProjectSelectItem
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public long IncubatorId { get; set; }
}
