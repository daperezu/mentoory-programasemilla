using System.Text;
using LinaSys.Auth.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace LinaSys.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailModel(IAuthRepository authRepository)
    : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (userId is null || code is null)
        {
            return RedirectToPage("/Index");
        }

        var user = await authRepository.FindUserByIdAsync(userId);
        if (user is null)
        {
            StatusMessage = "Error: Usuario no encontrado.";
            IsSuccess = false;
            return Page();
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await authRepository.ConfirmEmailAsync(user, code);

        if (!result.Success)
        {
            StatusMessage = "Error al confirmar el correo electrónico. El enlace puede haber expirado o ser inválido.";
            IsSuccess = false;
            return Page();
        }

        StatusMessage = "¡Gracias por confirmar tu correo electrónico!";
        IsSuccess = true;
        return Page();
    }
}
