using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(
    MediatR.IMediator mediatR,
    ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public Task OnGetAsync(string returnUrl = "")
    {
        ReturnUrl = returnUrl;
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = "")
    {
        returnUrl ??= Url.Content("~/");
        if (ModelState.IsValid)
        {
            var result = await mediatR.Send(new CreateUserCommand(
                Email: Input.Email,
                Password: Input.Password,
                Username: Input.Username,
                Identification: Input.Username,
                EmailConfirmed: false,
                IsTemporaryPassword: false)).ConfigureAwait(false);

            var userCreated = result.Value;
            var errors = result.ErrorMessages;

            if (userCreated is not null)
            {
                logger.LogInformation("User created a new account with password.");

                // Email will be sent automatically via UserAccountCreatedIntegrationEvent handler
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
            }

            foreach (var error in errors!)
            {
                ModelState.AddModelError(error.Context, error.Message);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    public class InputModel
    {
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y su confirmación no son iguales.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo electrónico válido.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de largo.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debes ingresar tu nombre completo tal como aparece en tu identificación.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Debes ingresar tu identificación sin guiones ni espacios")]
        public string Username { get; set; }
    }
}
