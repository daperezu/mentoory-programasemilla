using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Diagnostics.Application.Question.Commands;

/// <summary>
/// Command to delete a diagnostic question.
/// </summary>
public sealed record DeleteQuestionCommand(long Id) : IBaseRequest;

/// <summary>
/// Validator for DeleteQuestionCommand.
/// </summary>
public sealed class DeleteQuestionCommandValidator : AbstractValidator<DeleteQuestionCommand>
{
    public DeleteQuestionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de la pregunta debe ser válido.");
    }
}

/// <summary>
/// Handler for DeleteQuestionCommand.
/// </summary>
public sealed class DeleteQuestionCommandHandler(
    IQuestionRepository questionRepository,
    DiagnosticsDbContext dbContext,
    ILogger<DeleteQuestionCommandHandler> logger)
    : BaseCommandHandler<DeleteQuestionCommand>
{
    public override async Task<Result> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting question with ID: {QuestionId}", request.Id);

            // Get the existing question
            var question = await questionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (question is null)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(request.Id), "La pregunta no existe."));
            }

            // Check if the question is being used in any form
            var isInUse = await questionRepository.IsQuestionInUseAsync(request.Id, cancellationToken);
            if (isInUse)
            {
                return Failure(
                    ResultErrorCodes.GenericError,
                    (nameof(request.Id), "No se puede eliminar la pregunta porque está siendo utilizada en uno o más formularios."));
            }

            // Delete the question
            questionRepository.Delete(question);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Question deleted successfully with ID: {QuestionId}", request.Id);
            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting question with ID: {QuestionId}", request.Id);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(request.Id), "Error al eliminar la pregunta."));
        }
    }
}