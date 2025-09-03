using FluentValidation;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;

/// <summary>
/// Validator for GetProjectFormStructureQuery.
/// </summary>
public sealed class GetProjectFormStructureQueryValidator : AbstractValidator<GetProjectFormStructureQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProjectFormStructureQueryValidator"/> class.
    /// </summary>
    public GetProjectFormStructureQueryValidator()
    {
        RuleFor(query => query.ProjectId)
            .GreaterThan(0)
            .WithMessage("El ID del proyecto debe ser mayor que 0.");

        RuleFor(query => query.FormId)
            .GreaterThan(0)
            .WithMessage("El ID del formulario debe ser mayor que 0.");
    }
}