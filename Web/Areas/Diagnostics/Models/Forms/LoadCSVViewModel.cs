using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Diagnostics.Models.Forms;

public class LoadCSVViewModel
{
    [Required]
    public string FormName { get; set; }

    [Required]
    public IFormFile File { get; set; }
}
