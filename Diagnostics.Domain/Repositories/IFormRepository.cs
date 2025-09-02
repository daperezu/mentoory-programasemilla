using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Repositories;

public interface IFormRepository : IRepository<Form>
{
    Form Add(Form form);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

    ValueTask<Form?> FindByIdAsync(long requestFormId, CancellationToken cancellationToken);

    Task<List<long>> GetBlockIdsByFormIdAsync(long formId, CancellationToken cancellationToken);

    Task<Form?> GetByIdWithBlocksQuestionsAndAnswersAsync(long formId, CancellationToken cancellationToken);

    Task<Form?> GetByIdWithQuestions(long formId, CancellationToken cancellationToken);

    Task<Question?> GetQuestionByIdWithAnswerOptions(long questionId, CancellationToken cancellationToken);
}
