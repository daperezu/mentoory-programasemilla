using FluentValidation;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncAllProjectKnowledgeStructure;

/// <summary>
/// Command to synchronize all non-customized elements in a project's knowledge structure.
/// </summary>
public sealed record SyncAllProjectKnowledgeStructureCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId) : IBaseRequest;

/// <summary>
/// Validator for SyncAllProjectKnowledgeStructureCommand.
/// </summary>
internal sealed class SyncAllProjectKnowledgeStructureCommandValidator : AbstractValidator<SyncAllProjectKnowledgeStructureCommand>
{
    public SyncAllProjectKnowledgeStructureCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("El ID de la incubadora es requerido.");

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");
    }
}