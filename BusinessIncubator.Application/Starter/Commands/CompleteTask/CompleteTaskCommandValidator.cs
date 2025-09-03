using FluentValidation;

namespace LinaSys.BusinessIncubator.Application.Starter.Commands.CompleteTask;

/// <summary>
/// Validator for CompleteTaskCommand.
/// </summary>
public sealed class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteTaskCommandValidator"/> class.
    /// </summary>
    public CompleteTaskCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido.")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder 450 caracteres.");

        RuleFor(command => command.TaskId)
            .GreaterThan(0)
            .WithMessage("El ID de la tarea debe ser mayor que 0.");

        RuleFor(command => command.CompletionNotes)
            .MaximumLength(1000)
            .WithMessage("Las notas de finalización no pueden exceder 1000 caracteres.")
            .When(command => !string.IsNullOrEmpty(command.CompletionNotes));
    }
}