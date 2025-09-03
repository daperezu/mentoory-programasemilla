using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;

public class CreateViewModel
{
    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Key { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public long? PackageVersionId { get; set; }

    public List<SelectListItem> PackageOptions { get; set; } = [];
}
