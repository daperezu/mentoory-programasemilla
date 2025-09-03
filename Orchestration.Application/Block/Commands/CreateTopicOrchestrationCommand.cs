using FluentValidation;
using LinaSys.KnowledgeStructure.Application.Topic.Commands;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Orchestration.Application.Block.Commands;

public sealed record CreateTopicOrchestrationCommand(
    string Name,
    long ModuleId) : IBaseRequest<long>;

public class CreateTopicOrchestrationCommandValidator : AbstractValidator<CreateTopicOrchestrationCommand>
{
    public CreateTopicOrchestrationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("ModuleId must be a valid positive identifier.");
    }
}

public sealed class CreateTopicOrchestrationCommandHandler(
    IMediator mediator,
    IModuleRepository moduleRepository) : BaseCommandHandler<CreateTopicOrchestrationCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateTopicOrchestrationCommand request,
        CancellationToken cancellationToken)
    {
        // Verify the module exists (cross-aggregate validation)
        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.ModuleId), "Referenced module does not exist."));
        }

        // Create the topic through its own aggregate
        var createTopicCommand = new CreateTopicCommand(request.Name, null);
        var topicResult = await mediator.Send(createTopicCommand, cancellationToken);

        if (!topicResult.IsSuccess)
        {
            return Failure(topicResult.ErrorCode ?? ResultErrorCodes.Unknown, topicResult.ErrorMessages ?? []);
        }

        return Success(topicResult.Value);
    }
}
