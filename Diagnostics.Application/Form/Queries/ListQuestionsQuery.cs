using System.Linq.Expressions;
using FluentValidation;
using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Extensions;
using LinaSys.Shared.Application.MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinaSys.Diagnostics.Application.Form.Queries;

public record ListQuestionsQuery(
    int Start,
    int Length,
    long? FormId,
    string? Text,
    int? AnswerType,
    int? AppliesToPhase,
    bool? IsUsedForMentoringPlan,
    string? OrderByColumn,
    string? OrderDirection) : IBaseRequest<FilteredQueryResult<QuestionListItemDto>>;

public record QuestionListItemDto(
    long Id,
    string Text,
    int AnswerType,
    int AppliesToPhase,
    bool IsUsedForMentoringPlan,
    int TotalAnswers);

public class ListQuestionsQueryValidator : AbstractValidator<ListQuestionsQuery>
{
    public ListQuestionsQueryValidator()
    {
        RuleFor(x => x.Start).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Length).GreaterThan(0);
    }
}

public partial class ListQuestionsQueryHandler(ILogger<ListQuestionsQueryHandler> logger, DiagnosticsDbContext context)
    : BaseCommandHandler<ListQuestionsQuery, FilteredQueryResult<QuestionListItemDto>>
{
    public IQueryable<LinaSys.Diagnostics.Domain.Aggregates.Form.Question> ApplyFilters(
        IQueryable<LinaSys.Diagnostics.Domain.Aggregates.Form.Question> query,
        ListQuestionsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            query = query.Where(b => b.Text.Contains(request.Text.ToLower(), StringComparison.InvariantCultureIgnoreCase));
        }

        if (request.AnswerType.HasValue)
        {
            query = query.Where(b => b.AnswerType == (AnswerType)request.AnswerType.Value);
        }

        if (request.AppliesToPhase.HasValue)
        {
            query = query.Where(b => b.AppliesToPhase == (QuestionPhase)request.AppliesToPhase.Value);
        }

        if (request.IsUsedForMentoringPlan.HasValue)
        {
            query = query.Where(b => b.IsUsedForMentoringPlan == request.IsUsedForMentoringPlan.Value);
        }

        if (request.FormId.HasValue)
        {
            // Join with FormQuestions table to filter by FormId
            query = query.Where(q => context.Set<FormQuestion>().Any(fq => fq.QuestionId == q.Id && fq.FormId == request.FormId.Value));
        }

        return query;
    }

    public override async Task<Result<FilteredQueryResult<QuestionListItemDto>>> Handle(ListQuestionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var baseQuery = context.Questions.AsNoTracking();

            var total = await baseQuery.CountAsync(cancellationToken);

            var filteredQuery = ApplyFilters(baseQuery, request);

            var totalFiltered = await filteredQuery.CountAsync(cancellationToken);

            var orderingMap = new Dictionary<string, Expression<Func<LinaSys.Diagnostics.Domain.Aggregates.Form.Question, object?>>>
            {
                ["text"] = b => b.Text,
            };

            var paged = filteredQuery
                .ApplyOrdering(request.OrderByColumn, request.OrderDirection, orderingMap)
                .ApplyPaging(request.Start, request.Length);

            var data = await paged
                .Select(b => new QuestionListItemDto(
                    b.Id,
                    b.Text,
                    (int)b.AnswerType,
                    (int)b.AppliesToPhase,
                    b.IsUsedForMentoringPlan,
                    b.AnswerOptions.Count))
                .ToListAsync(cancellationToken);

            return Success(FilteredQueryResult.From(data, total, totalFiltered));
        }
        catch (Exception ex)
        {
            LogQueryFailed(ex.Message);
            return Failure(ResultErrorCodes.BusinessIncubator_FilterQueryFailed, (nameof(ListQuestionsQuery), ex.Message));
        }
    }

    [LoggerMessage(EventId = 800_090, Level = LogLevel.Error, Message = nameof(ListQuestionsQueryHandler) + " failed with error: {ErrorMessage}")]
    partial void LogQueryFailed(string errorMessage);
}
