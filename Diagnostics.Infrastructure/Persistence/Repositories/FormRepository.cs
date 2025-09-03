using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Diagnostics.Infrastructure.Persistence.Repositories;

public class FormRepository(DiagnosticsDbContext dbContext)
    : AbstractRepository<Form>(dbContext), IFormRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return dbContext.Forms
            .AsNoTracking()
            .AnyAsync(x => x.Name == name, cancellationToken);
    }

    public Task<Form?> GetByIdWithQuestions(long formId, CancellationToken cancellationToken)
    {
        return dbContext.Forms
            .Include(x => x.FormQuestions)
            .FirstOrDefaultAsync(x => x.Id == formId, cancellationToken);
    }

    public Task<Question?> GetQuestionByIdWithAnswerOptions(long questionId, CancellationToken cancellationToken)
    {
        return dbContext.Questions
            .Include(x => x.AnswerOptions)
            .FirstOrDefaultAsync(x => x.Id == questionId, cancellationToken);
    }

    public Task<List<long>> GetBlockIdsByFormIdAsync(long formId, CancellationToken cancellationToken)
    {
        return dbContext.Forms
            .Where(x => x.Id == formId)
            .SelectMany(x => x.FormQuestions.Select(q => q.BlockId))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public Task<Form?> GetByIdWithBlocksQuestionsAndAnswersAsync(long formId, CancellationToken cancellationToken)
    {
        return dbContext.Forms
            .Include(x => x.FormQuestions)
                .ThenInclude(q => q.Block)
            .Include(x => x.FormQuestions)
                .ThenInclude(q => q.Question)
                    .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(x => x.Id == formId, cancellationToken);
    }
}
