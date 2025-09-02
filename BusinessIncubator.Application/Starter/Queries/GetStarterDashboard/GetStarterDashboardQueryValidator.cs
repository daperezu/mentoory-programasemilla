using FluentValidation;

namespace LinaSys.BusinessIncubator.Application.Starter.Queries.GetStarterDashboard;

/// <summary>
/// Validator for GetStarterDashboardQuery.
/// </summary>
public sealed class GetStarterDashboardQueryValidator : AbstractValidator<GetStarterDashboardQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetStarterDashboardQueryValidator"/> class.
    /// </summary>
    public GetStarterDashboardQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido.")
            .MaximumLength(450)
            .WithMessage("El ID del usuario no puede exceder 450 caracteres.");

        RuleFor(query => query.ProjectId)
            .GreaterThan(0)
            .WithMessage("El ID del proyecto debe ser mayor que 0.");
    }
}