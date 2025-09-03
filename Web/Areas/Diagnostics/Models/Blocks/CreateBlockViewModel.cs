using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Diagnostics.Models.Blocks;

public class CreateBlockViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [Display(Name = "Nombre")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    public string Name { get; set; } = string.Empty;
}
