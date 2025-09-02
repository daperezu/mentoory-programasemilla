using FluentValidation;

namespace LinaSys.Core.Application.Dashboard.Commands.UpdatePreferences;

/// <summary>
/// Validator for UpdatePreferencesCommand.
/// </summary>
public sealed class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
{
    private readonly string[] _validThemes = ["light", "dark", "auto"];
    private readonly string[] _validLanguages = ["es", "en"];
    private readonly string[] _validDateFormats = ["DD/MM/YYYY", "MM/DD/YYYY", "YYYY-MM-DD"];
    private readonly string[] _validTimeFormats = ["12h", "24h"];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePreferencesCommandValidator"/> class.
    /// </summary>
    public UpdatePreferencesCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido.")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder 450 caracteres.");

        RuleFor(command => command.Theme)
            .NotEmpty()
            .WithMessage("El tema es requerido.")
            .Must(theme => _validThemes.Contains(theme.ToLower()))
            .WithMessage("El tema debe ser 'light', 'dark' o 'auto'.");

        RuleFor(command => command.Language)
            .NotEmpty()
            .WithMessage("El idioma es requerido.")
            .Must(lang => _validLanguages.Contains(lang.ToLower()))
            .WithMessage("El idioma debe ser 'es' o 'en'.");

        RuleFor(command => command.RefreshInterval)
            .InclusiveBetween(5, 3600)
            .WithMessage("El intervalo de actualización debe estar entre 5 y 3600 segundos.");

        RuleFor(command => command.DateFormat)
            .NotEmpty()
            .WithMessage("El formato de fecha es requerido.")
            .Must(format => _validDateFormats.Contains(format))
            .WithMessage("Formato de fecha inválido.");

        RuleFor(command => command.TimeFormat)
            .NotEmpty()
            .WithMessage("El formato de hora es requerido.")
            .Must(format => _validTimeFormats.Contains(format))
            .WithMessage("El formato de hora debe ser '12h' o '24h'.");

        RuleFor(command => command.Timezone)
            .NotEmpty()
            .WithMessage("La zona horaria es requerida.")
            .MaximumLength(100)
            .WithMessage("La zona horaria no puede exceder 100 caracteres.");
    }
}