using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class IdempotencyRepository(AppDbContext dbContext)
    : Repository<IdempotencyRecord>(dbContext), IIdempotencyRepository
{
    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.IdempotencyRecords
            .Where(record => record.Id == id)
            .ExecuteDeleteAsync(cancellationToken) > 0;
    }

    public async Task<int> DeleteExpiredAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return await dbContext.IdempotencyRecords
            .Where(record => record.ExpiresAt <= now)
            .ExecuteDeleteAsync(cancellationToken);
    }
}