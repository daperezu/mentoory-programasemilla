using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectStages;

public class ProjectStagesViewModel
{
    public Guid BusinessIncubatorId { get; set; }

    public string BusinessIncubatorName { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public List<StageViewModel> Stages { get; set; } = [];
}

public class StageViewModel
{
    public long Id { get; set; }

    public ProjectStageType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

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

    public string StatusDisplay => IsActive ? "Activa" : "Inactiva";

    public string StatusBadgeClass => IsActive ? "badge-success" : "badge-secondary";

    public bool CanActivate => !IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;

    public bool CanDeactivate => IsActive;
}