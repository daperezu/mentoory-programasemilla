using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Diagnostics.Models.Forms;

public class CreateFormViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [Display(Name = "Nombre del Formulario")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string Name { get; set; } = string.Empty;
}
