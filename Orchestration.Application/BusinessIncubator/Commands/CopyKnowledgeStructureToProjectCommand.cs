using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using IBaseRequest = LinaSys.Shared.Application.MediatR.IBaseRequest;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to copy a knowledge structure to a project from different sources.
/// </summary>
public record CopyKnowledgeStructureToProjectCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    KnowledgeStructureSourceType SourceType,
    long SourceId) : IBaseRequest;

/// <summary>
/// Validator for CopyKnowledgeStructureToProjectCommand.
/// </summary>
public class CopyKnowledgeStructureToProjectCommandValidator : AbstractValidator<CopyKnowledgeStructureToProjectCommand>
{
    public CopyKnowledgeStructureToProjectCommandValidator()
    {
        RuleFor(command => command.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator External ID must not be empty.");

        RuleFor(command => command.ProjectExternalId)
            .NotEmpty()
            .WithMessage("Project External ID must not be empty.");

        RuleFor(command => command.SourceType)
            .IsInEnum()
            .WithMessage("Source type must be valid.");

        RuleFor(command => command.SourceId)
            .GreaterThan(0)
            .WithMessage("Source ID must be greater than 0.");
    }
}

/// <summary>
/// Handler for CopyKnowledgeStructureToProjectCommand.
/// </summary>
public class CopyKnowledgeStructureToProjectCommandHandler(IMediator mediator) : BaseCommandHandler<CopyKnowledgeStructureToProjectCommand>
{
    public override async Task<Result> Handle(CopyKnowledgeStructureToProjectCommand request, CancellationToken cancellationToken)
    {
        // Route to the appropriate handler based on source type
        if (request.SourceType == KnowledgeStructureSourceType.Global)
        {
            // Copy from diagnostics form
            var command = new CopyDiagnosticsFormToBusinessIncubatorProjectCommand(
                request.BusinessIncubatorExternalId,
                request.SourceId,
                request.ProjectExternalId);

            return await mediator.Send(command, cancellationToken);
        }
        else if (request.SourceType == KnowledgeStructureSourceType.Project)
        {
            // Copy from another project within the same incubator
            var command = new CopyProjectKnowledgeStructureToProjectCommand(
                request.BusinessIncubatorExternalId,
                request.SourceId,
                request.ProjectExternalId);

            return await mediator.Send(command, cancellationToken);
        }
        else
        {
            return Failure(ResultErrorCodes.Unknown, ("SourceType", "Tipo de origen no válido"));
        }
    }
}