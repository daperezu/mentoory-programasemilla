using LinaSys.Diagnostics.Domain.Aggregates.Form;

namespace LinaSys.Diagnostics.Domain.Repositories;

/// <summary>
/// Repository interface for Question operations.
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// Adds a new question to the repository.
    /// </summary>
    /// <returns></returns>
    Question Add(Question question);

    /// <summary>
    /// Updates an existing question.
    /// </summary>
    void Update(Question question);

    /// <summary>
    /// Deletes a question.
    /// </summary>
    void Delete(Question question);

    /// <summary>
    /// Gets a question by ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<Question?> GetByIdAsync(long questionId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a question by ID with its answer options.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<Question?> GetByIdWithAnswerOptionsAsync(long questionId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a question is being used in any form.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> IsQuestionInUseAsync(long questionId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all questions with their relationships for listing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<List<Question>> GetAllWithRelationshipsAsync(CancellationToken cancellationToken);
}