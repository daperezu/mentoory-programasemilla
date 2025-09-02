using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Diagnostics.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Diagnostics.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Question operations.
/// </summary>
public class QuestionRepository(DiagnosticsDbContext dbContext) : IQuestionRepository
{
    /// <inheritdoc/>
    public Question Add(Question question)
    {
        return dbContext.Questions.Add(question).Entity;
    }

    /// <inheritdoc/>
    public void Update(Question question)
    {
        dbContext.Questions.Update(question);
    }

    /// <inheritdoc/>
    public void Delete(Question question)
    {
        dbContext.Questions.Remove(question);
    }

    /// <inheritdoc/>
    public async Task<Question?> GetByIdAsync(long questionId, CancellationToken cancellationToken)
    {
        return await dbContext.Questions
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Question?> GetByIdWithAnswerOptionsAsync(long questionId, CancellationToken cancellationToken)
    {
        return await dbContext.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> IsQuestionInUseAsync(long questionId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<FormQuestion>()
            .AnyAsync(fq => fq.QuestionId == questionId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<Question>> GetAllWithRelationshipsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Questions
            .Include(q => q.AnswerOptions)
            .ToListAsync(cancellationToken);
    }
}