using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account.Manage;

public partial class IndexModel(
    IAuthRepository authRepository,
    SignInManager<User> signInManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public string Username { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await authRepository.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{authRepository.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await authRepository.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{authRepository.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var phoneNumber = await authRepository.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await authRepository.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Success)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }

    private async Task LoadAsync(User user)
    {
        var userName = await authRepository.GetUserNameAsync(user);
        var phoneNumber = await authRepository.GetPhoneNumberAsync(user);

        Username = userName!;

        Input = new InputModel
        {
            PhoneNumber = phoneNumber!,
        };
    }

    public class InputModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
