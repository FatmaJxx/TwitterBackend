using Domain.Entities;

namespace Domain.Interfaces;

public interface IRepository<T> where T : IBaseEntity
{
    Task<T?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllAsync(
        CancellationToken cancellationToken = default);

    T Add(T entity);

    T Update(T entity);

    void Delete(T entity);
}