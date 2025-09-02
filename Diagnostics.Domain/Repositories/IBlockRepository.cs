using LinaSys.Diagnostics.Domain.Aggregates.Block;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Diagnostics.Domain.Repositories;

public interface IBlockRepository : IRepository<Block>
{
    Block Add(Block block);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

    Task<List<Block>> GetAllBlocksAsync(CancellationToken cancellationToken);

    Task<List<(Block Block, int QuestionCount)>> GetAllBlocksWithQuestionCountAsync(CancellationToken cancellationToken);

    Task<List<Block>> GetBlocksByIdsAsync(List<long> ids, CancellationToken cancellationToken);

    Task<Block?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task<Block?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<Block?> GetBlockWithFormQuestionsAsync(long id, CancellationToken cancellationToken);

    Task<bool> IsBlockInUseAsync(long blockId, CancellationToken cancellationToken);

    void Remove(Block block);
}
