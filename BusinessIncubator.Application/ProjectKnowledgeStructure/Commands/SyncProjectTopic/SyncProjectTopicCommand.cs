using FluentValidation;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectTopic;

/// <summary>
/// Command to synchronize a project topic with its source.
/// </summary>
public sealed record SyncProjectTopicCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long TopicId) : IBaseRequest;

/// <summary>
/// Validator for SyncProjectTopicCommand.
/// </summary>
internal sealed class SyncProjectTopicCommandValidator : AbstractValidator<SyncProjectTopicCommand>
{
    public SyncProjectTopicCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("El ID de la incubadora es requerido.");

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.TopicId)
            .GreaterThan(0)
            .WithMessage("El ID del tema debe ser mayor que cero.");
    }
}