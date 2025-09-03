using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

public class EditUserViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

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
}
