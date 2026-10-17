using Domain.Entities;

namespace Domain.Interfaces;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<IdempotencyRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    IdempotencyRecord Add(IdempotencyRecord idempotencyRecord);

    IdempotencyRecord Update(IdempotencyRecord idempotencyRecord);

    void Delete(IdempotencyRecord idempotencyRecord);

    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> DeleteExpiredAsync(DateTime now, CancellationToken cancellationToken = default);
}