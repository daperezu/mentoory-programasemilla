using FluentValidation;

namespace LinaSys.Core.Application.Dashboard.Commands.UpdateWidgetLayout;

/// <summary>
/// Validator for UpdateWidgetLayoutCommand.
/// </summary>
public sealed class UpdateWidgetLayoutCommandValidator : AbstractValidator<UpdateWidgetLayoutCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWidgetLayoutCommandValidator"/> class.
    /// </summary>
    public UpdateWidgetLayoutCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido.")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder 450 caracteres.");

        RuleFor(command => command.WidgetLayouts)
            .NotNull()
            .WithMessage("La lista de layouts de widgets es requerida.")
            .Must(layouts => layouts is not null && layouts.Count > 0)
            .WithMessage("Debe proporcionar al menos un layout de widget.");

        RuleForEach(command => command.WidgetLayouts)
            .SetValidator(new WidgetLayoutItemValidator());
    }
}

/// <summary>
/// Validator for WidgetLayoutItem.
/// </summary>
public sealed class WidgetLayoutItemValidator : AbstractValidator<WidgetLayoutItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WidgetLayoutItemValidator"/> class.
    /// </summary>
    public WidgetLayoutItemValidator()
    {
        RuleFor(item => item.WidgetId)
            .GreaterThan(0)
            .WithMessage("El ID del widget debe ser mayor que 0.");

        RuleFor(item => item.Position)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La posición debe ser mayor o igual a 0.");

        RuleFor(item => item.Width)
            .InclusiveBetween(1, 12)
            .WithMessage("El ancho debe estar entre 1 y 12 columnas.");

        RuleFor(item => item.Height)
            .InclusiveBetween(1, 10)
            .WithMessage("La altura debe estar entre 1 y 10 filas.");

        RuleFor(item => item.Configuration)
            .MaximumLength(4000)
            .WithMessage("La configuración no puede exceder 4000 caracteres.")
            .When(item => !string.IsNullOrEmpty(item.Configuration));
    }
}