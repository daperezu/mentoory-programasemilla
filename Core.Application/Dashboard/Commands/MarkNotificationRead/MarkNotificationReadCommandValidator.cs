using FluentValidation;

namespace LinaSys.Core.Application.Dashboard.Commands.MarkNotificationRead;

/// <summary>
/// Validator for MarkNotificationReadCommand.
/// </summary>
public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkNotificationReadCommandValidator"/> class.
    /// </summary>
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido.")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder 450 caracteres.");

        RuleFor(command => command.NotificationId)
            .GreaterThan(0)
            .WithMessage("El ID de la notificación debe ser mayor que 0.");
    }
}

/// <summary>
/// Validator for MarkAllNotificationsReadCommand.
/// </summary>
public sealed class MarkAllNotificationsReadCommandValidator : AbstractValidator<MarkAllNotificationsReadCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkAllNotificationsReadCommandValidator"/> class.
    /// </summary>
    public MarkAllNotificationsReadCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido.")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder 450 caracteres.");
    }
}