using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account.Manage;

public class SetPasswordModel(
    IAuthRepository authRepository,
    SignInManager<User> signInManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await authRepository.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{authRepository.GetUserId(User)}'.");
        }

        var hasPassword = await authRepository.HasPasswordAsync(user);

        if (hasPassword)
        {
            return RedirectToPage("./ChangePassword");
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

        var addPasswordResult = await authRepository.AddPasswordAsync(user, Input.NewPassword);
        if (!addPasswordResult.Success)
        {
            foreach (var error in addPasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return Page();
        }

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your password has been set.";

        return RedirectToPage();
    }

    public class InputModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
    }
}
