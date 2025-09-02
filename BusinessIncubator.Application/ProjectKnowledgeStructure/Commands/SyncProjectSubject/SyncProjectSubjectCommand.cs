using FluentValidation;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectSubject;

/// <summary>
/// Command to synchronize a project subject with its source.
/// </summary>
public sealed record SyncProjectSubjectCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long SubjectId) : IBaseRequest;

/// <summary>
/// Validator for SyncProjectSubjectCommand.
/// </summary>
internal sealed class SyncProjectSubjectCommandValidator : AbstractValidator<SyncProjectSubjectCommand>
{
    public SyncProjectSubjectCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("El ID de la incubadora es requerido.");

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0)
            .WithMessage("El ID de la materia debe ser mayor que cero.");
    }
}