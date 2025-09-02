using FluentValidation;

namespace LinaSys.UserManagement.Application.Commands.CreateUserProfile;

public class CreateUserProfileCommandValidator : AbstractValidator<CreateUserProfileCommand>
{
    public CreateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres");

        RuleFor(x => x.Identification)
            .NotEmpty().WithMessage("La identificación es requerida")
            .MaximumLength(50).WithMessage("La identificación no puede exceder 50 caracteres");
    }
}