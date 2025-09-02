using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

public sealed record ValidateProjectQuestionsQuery(
    Guid ProjectExternalId,
    List<long> QuestionIds) : IBaseRequest<Dictionary<long, QuestionValidationData>>;

public sealed record QuestionValidationData(
    long QuestionId,
    bool IsRequired,
    List<long> ValidAnswerOptionIds);

public sealed class ValidateProjectQuestionsQueryHandler(
    IBusinessIncubatorRepository repository) : BaseCommandHandler<ValidateProjectQuestionsQuery, Dictionary<long, QuestionValidationData>>
{
    public override async Task<Result<Dictionary<long, QuestionValidationData>>> Handle(
        ValidateProjectQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var validationData = await repository.ValidateProjectQuestionsAsync(
            request.ProjectExternalId,
            request.QuestionIds,
            cancellationToken);

        if (validationData is null || !validationData.Any())
        {
            return Failure(ResultErrorCodes.Project_NotFound, ("ProjectExternalId", "Proyecto no encontrado"));
        }

        var result = validationData.ToDictionary(
            kvp => kvp.Key,
            kvp => new QuestionValidationData(
                kvp.Key,
                true, // IsRequired - default to true for validation
                kvp.Value.ValidAnswerOptionIds));

        return Success(result);
    }
}