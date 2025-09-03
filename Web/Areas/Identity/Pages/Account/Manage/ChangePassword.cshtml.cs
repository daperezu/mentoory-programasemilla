using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.UserManagement.Application.Commands.UpdateUserPreferences;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel(
    IAuthRepository authRepository,
    SignInManager<User> signInManager,
    ILogger<ChangePasswordModel> logger,
    IMediator mediator) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public bool IsEnforcedPasswordChange { get; set; }

    public async Task<IActionResult> OnGetAsync(bool enforced = false)
    {
        var user = await authRepository.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{authRepository.GetUserId(User)}'.");
        }

        var hasPassword = await authRepository.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return RedirectToPage("./SetPassword");
        }

        IsEnforcedPasswordChange = enforced;
        if (enforced)
        {
            StatusMessage = "Por seguridad, debes cambiar tu contraseña temporal antes de continuar.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await authRepository.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{authRepository.GetUserId(User)}'.");
        }

        var changePasswordResult = await authRepository.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Success)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return Page();
        }

        // Clear the requires_password_change preference after successful password change
        var clearPrefCommand = new UpdateUserPreferencesCommand(
            UserId: user.Id,
            Preferences: new Dictionary<string, string>
            {
                ["auth.requires_password_change"] = "false"
            });

        var prefResult = await mediator.Send(clearPrefCommand);
        if (!prefResult.IsSuccess)
        {
            logger.LogWarning("Failed to clear password change requirement for user {UserId}", user.Id);
        }

        await signInManager.RefreshSignInAsync(user);
        logger.LogInformation("User changed their password successfully.");
        StatusMessage = "Tu contraseña ha sido cambiada exitosamente.";

        // If this was an enforced password change, redirect to dashboard
        if (Request.Query.ContainsKey("enforced"))
        {
            return RedirectToAction("RedirectToDashboard", "AuthRedirect", new { area = string.Empty });
        }

        return RedirectToPage();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
    }
}
