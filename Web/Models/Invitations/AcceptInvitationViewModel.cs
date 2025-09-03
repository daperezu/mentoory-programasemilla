using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Models.Invitations;

public class AcceptInvitationViewModel
{
    public string Token { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva Contraseña")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Contraseña")]
    [Compare("NewPassword", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
    [Display(Name = "Acepto los términos y condiciones")]
    public bool AcceptTerms { get; set; }
}

public class InvitationExpiredViewModel
{
    public string ProjectName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}
