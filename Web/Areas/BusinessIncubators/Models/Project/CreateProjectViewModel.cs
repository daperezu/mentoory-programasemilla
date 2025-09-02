using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.Project;

public class CreateProjectViewModel
{
    public string BusinessIncubatorName { get; set; } = "- unknown -";

    public string? Description { get; set; }

    [Required]
    public string Key { get; set; }

    [Required]
    public string Name { get; set; }
}
