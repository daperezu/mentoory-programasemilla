using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel
    : PageModel
{
    public string Email { get; set; }

    public IActionResult OnGet(string email, string returnUrl = "")
    {
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Index");
        }

        Email = email;

        return Page();
    }
}
