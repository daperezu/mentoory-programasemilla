using FluentValidation;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetParticipantSubmissions;

/// <summary>
/// Validator for GetParticipantSubmissionsQuery.
/// </summary>
public sealed class GetParticipantSubmissionsQueryValidator : AbstractValidator<GetParticipantSubmissionsQuery>
{
    public GetParticipantSubmissionsQueryValidator()
    {
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.ParticipantUserId)
            .NotEmpty()
            .WithMessage("El ID del participante es requerido.");
    }
}