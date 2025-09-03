using LinaSys.Diagnostics.Domain.Aggregates.Block;
using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Diagnostics.Infrastructure.Persistence.Repositories;

public class BlockRepository(DiagnosticsDbContext dbContext)
    : AbstractRepository<Block>(dbContext), IBlockRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return dbContext.Blocks
            .AsNoTracking()
            .AnyAsync(x => x.Name == name, cancellationToken);
    }

    public Task<List<Block>> GetAllBlocksAsync(CancellationToken cancellationToken)
    {
        return dbContext.Blocks
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<(Block Block, int QuestionCount)>> GetAllBlocksWithQuestionCountAsync(CancellationToken cancellationToken)
    {
        var blocksWithCounts = await dbContext.Blocks
            .AsNoTracking()
            .Select(b => new
            {
                Block = b,
                QuestionCount = dbContext.Set<FormQuestion>().Count(fq => fq.BlockId == b.Id),
            })
            .ToListAsync(cancellationToken);

        return blocksWithCounts.Select(x => (x.Block, x.QuestionCount)).ToList();
    }

    public Task<List<Block>> GetBlocksByIdsAsync(List<long> ids, CancellationToken cancellationToken)
    {
        return dbContext.Blocks
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Block?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return dbContext.Blocks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public Task<Block?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return dbContext.Blocks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Block?> GetBlockWithFormQuestionsAsync(long id, CancellationToken cancellationToken)
    {
        // Note: We can't include FormQuestions directly as it's internal
        // The check for whether a block is in use is done via IsBlockInUseAsync
        return dbContext.Blocks
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsBlockInUseAsync(long blockId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<FormQuestion>()
            .AnyAsync(fq => fq.BlockId == blockId, cancellationToken);
    }

    public void Remove(Block block)
    {
        dbContext.Blocks.Remove(block);
    }
}
