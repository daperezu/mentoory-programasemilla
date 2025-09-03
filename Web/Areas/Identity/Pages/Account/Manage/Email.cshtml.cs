using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace LinaSys.Web.Areas.Identity.Pages.Account.Manage;

public partial class EmailModel(
    IAuthRepository authRepository,
    IEmailSender emailSender) : PageModel
{
    public string Email { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public bool IsEmailConfirmed { get; set; }

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

    public async Task<IActionResult> OnPostChangeEmailAsync()
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

        var email = await authRepository.GetEmailAsync(user);
        if (Input.NewEmail != email)
        {
            var userId = await authRepository.GetUserIdAsync(user);
            var code = await authRepository.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { userId = userId, email = Input.NewEmail, code = code },
                protocol: Request.Scheme);
            await emailSender.SendEmailAsync(
                Input.NewEmail,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            StatusMessage = "Confirmation link to change email sent. Please check your email.";
            return RedirectToPage();
        }

        StatusMessage = "Your email is unchanged.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
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

        var userId = await authRepository.GetUserIdAsync(user);
        var email = await authRepository.GetEmailAsync(user);
        var code = await authRepository.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId = userId, code = code },
            protocol: Request.Scheme);
        await emailSender.SendEmailAsync(
            email!,
            "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToPage();
    }

    private async Task LoadAsync(User user)
    {
        var email = await authRepository.GetEmailAsync(user);
        Email = email!;

        Input = new InputModel
        {
            NewEmail = email!,
        };

        IsEmailConfirmed = await authRepository.IsEmailConfirmedAsync(user);
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }
    }
}
