using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel(IMediator mediator) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            // Request password reset - this will send email via integration event
            var result = await mediator.Send(new RequestPasswordResetByEmailCommand(Input.Email));

            // Always redirect to confirmation page regardless of result (for security)
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
