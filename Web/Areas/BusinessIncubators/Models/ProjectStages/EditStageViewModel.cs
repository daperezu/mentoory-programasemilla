using System.ComponentModel.DataAnnotations;
using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectStages;

public class EditStageViewModel
{
    public Guid BusinessIncubatorId { get; set; }

    public string BusinessIncubatorName { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public long StageId { get; set; }

    public ProjectStageType Type { get; set; }

    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres")]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    [Display(Name = "Fecha de Inicio")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de fin es requerida")]
    [Display(Name = "Fecha de Fin")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Display(Name = "Activa")]
    public bool IsActive { get; set; }

    public string TypeDisplay => Type switch
    {
        ProjectStageType.Invitation => "Invitación",
        ProjectStageType.InitialFormCollection => "Formulario Inicial",
        ProjectStageType.Mentoring => "Mentoría",
        ProjectStageType.FinalFormCollection => "Formulario Final",
        ProjectStageType.Closure => "Cierre",
        _ => Type.ToString(),
    };
}