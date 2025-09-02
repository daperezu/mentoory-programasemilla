using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.UserManagement.Application.Commands.UpdateUserPreferences;
using LinaSys.UserManagement.Application.Queries.GetUserPreferences;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LinaSys.Web.Areas.Identity.Pages.Account.Manage;

public class EmailPreferencesModel(
    UserManager<User> userManager,
    IMediator mediator,
    ILogger<EmailPreferencesModel> logger)
    : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{userManager.GetUserId(User)}'.");
        }

        await LoadPreferencesAsync(user.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPreferencesAsync(user.Id);
            return Page();
        }

        try
        {
            // Convert the InputModel to a dictionary of preferences
            var preferences = new Dictionary<string, string>
            {
                ["email.system.welcome"] = Input.SystemWelcome.ToString(),
                ["email.project.welcome"] = Input.ProjectWelcome.ToString(),
                ["email.form.approved"] = Input.Approvals.ToString(),
                ["email.form.rejected"] = Input.Rejections.ToString(),
                ["email.reminders"] = Input.Reminders.ToString(),
                ["email.announcements"] = Input.Announcements.ToString(),
                ["email.task.assignments"] = Input.TaskAssignments.ToString(),
                ["email.form.deadlines"] = Input.FormDeadlines.ToString(),
                ["email.mentor.messages"] = Input.MentorMessages.ToString(),
                ["email.digest"] = Input.Digest.ToString()
            };

            var command = new UpdateUserPreferencesCommand(user.Id, preferences);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                StatusMessage = "Tus preferencias de notificación han sido actualizadas exitosamente.";
                logger.LogInformation("User {UserId} updated their email preferences", user.Id);
            }
            else
            {
                StatusMessage = "Error: No se pudieron actualizar las preferencias. Por favor, intenta de nuevo.";
                logger.LogWarning("Failed to update email preferences for user {UserId}", user.Id);
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating email preferences for user {UserId}", user.Id);
            StatusMessage = "Error: Ocurrió un error inesperado. Por favor, intenta de nuevo.";
            return RedirectToPage();
        }
    }

    private static bool GetBoolPreference(Dictionary<string, string> preferences, string key, bool defaultValue)
    {
        if (preferences.TryGetValue(key, out var value))
        {
            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue;
            }
        }

        return defaultValue;
    }

    private async Task LoadPreferencesAsync(string userId)
    {
        try
        {
            var query = new GetUserPreferencesQuery(userId);
            var result = await mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var preferences = result.Value;

                // Map preferences to InputModel
                Input.SystemWelcome = GetBoolPreference(preferences, "email.system.welcome", true);
                Input.ProjectWelcome = GetBoolPreference(preferences, "email.project.welcome", true);
                Input.Approvals = GetBoolPreference(preferences, "email.form.approved", true);
                Input.Rejections = GetBoolPreference(preferences, "email.form.rejected", true);
                Input.Reminders = GetBoolPreference(preferences, "email.reminders", true);
                Input.Announcements = GetBoolPreference(preferences, "email.announcements", true);
                Input.TaskAssignments = GetBoolPreference(preferences, "email.task.assignments", true);
                Input.FormDeadlines = GetBoolPreference(preferences, "email.form.deadlines", true);
                Input.MentorMessages = GetBoolPreference(preferences, "email.mentor.messages", true);
                Input.Digest = GetBoolPreference(preferences, "email.digest", false);
            }
            else
            {
                // Set default values if no preferences exist
                Input = new InputModel();
                logger.LogInformation("No existing preferences found for user {UserId}, using defaults", userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading email preferences for user {UserId}", userId);
            // Use default values on error
            Input = new InputModel();
        }
    }

    public class InputModel
    {
        [Display(Name = "Email de bienvenida al sistema")]
        public bool SystemWelcome { get; set; } = true;

        [Display(Name = "Email de bienvenida al proyecto")]
        public bool ProjectWelcome { get; set; } = true;

        [Display(Name = "Notificaciones de aprobación")]
        public bool Approvals { get; set; } = true;

        [Display(Name = "Notificaciones de rechazo")]
        public bool Rejections { get; set; } = true;

        [Display(Name = "Recordatorios")]
        public bool Reminders { get; set; } = true;

        [Display(Name = "Anuncios del sistema")]
        public bool Announcements { get; set; } = true;

        [Display(Name = "Asignación de tareas")]
        public bool TaskAssignments { get; set; } = true;

        [Display(Name = "Fechas límite de formularios")]
        public bool FormDeadlines { get; set; } = true;

        [Display(Name = "Mensajes de mentores")]
        public bool MentorMessages { get; set; } = true;

        [Display(Name = "Resumen diario")]
        public bool Digest { get; set; } = false;
    }
}
