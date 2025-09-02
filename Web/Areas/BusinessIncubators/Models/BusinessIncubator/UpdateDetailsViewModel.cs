using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;

public class UpdateDetailsViewModel
{
    [Required]
    public Guid Id { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Key { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}
