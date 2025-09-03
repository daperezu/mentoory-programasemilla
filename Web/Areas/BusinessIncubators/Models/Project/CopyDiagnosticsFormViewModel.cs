using System.ComponentModel.DataAnnotations;
using LinaSys.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.Project;

public class CopyDiagnosticsFormViewModel : RestorableViewModel
{
    [Required(ErrorMessage = "La incubadora de negocios es requerida.")]
    public Guid BusinessIncubatorExternalId { get; set; }

    [Required(ErrorMessage = "El formulario de diagnóstico es requerido.")]
    [Display(Name = "Formulario de diagnóstico")]
    public long FormId { get; set; }

    [Required(ErrorMessage = "El proyecto es requerido.")]
    [Display(Name = "Proyecto destino")]
    public Guid ProjectExternalId { get; set; }

    [Display(Name = "Limpiar contenido del proyecto antes de copiar")]
    public bool ResetProjectBeforeCopy { get; set; }

    // Display properties
    public string BusinessIncubatorName { get; set; } = string.Empty;

    public string? SelectedFormName { get; set; }

    public string? SelectedProjectName { get; set; }

    // Dropdown options
    public List<SelectListItem> FormOptions { get; set; } = [];

    public List<SelectListItem> ProjectOptions { get; set; } = [];
}
