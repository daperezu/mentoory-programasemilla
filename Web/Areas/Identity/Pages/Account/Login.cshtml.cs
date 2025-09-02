using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Domain.AggregatesModel.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel(ILogger<LoginModel> logger, SignInManager<User> signInManager)
    : PageModel
{
    [TempData]
    public string ErrorMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public async Task OnGetAsync(string returnUrl = "")
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = "/")
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            logger.LogInformation("Login attempt for Identification: {Identification}", Input.Identification);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await signInManager.PasswordSignInAsync(Input.Identification, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                logger.LogInformation("User logged in.");

                // If no return URL specified, redirect to role-based dashboard
                // The RequirePasswordChangeFilter will handle password change enforcement on the next request
                if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
                {
                    return RedirectToAction("RedirectToDashboard", "AuthRedirect", new { area = string.Empty });
                }

                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }

            if (result.IsLockedOut)
            {
                logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                logger.LogWarning("Login failed for Identification: {Identification}", Input.Identification);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    public class InputModel
    {
        [Required]
        public string Identification { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
