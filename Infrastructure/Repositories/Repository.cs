using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal abstract class Repository<T>(AppDbContext dbContext) :
    IRepository<T> where T : class, IBaseEntity
{
    protected readonly AppDbContext DbContext = dbContext;

    public T Add(T entity)
    {
        DbContext.Set<T>().Add(entity);
        return entity;
    }

    public void Delete(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }

    public T Update(T entity)
    {
        DbContext.Set<T>().Update(entity);
        return entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbContext.Set<T>().ToListAsync(cancellationToken);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbContext.Set<T>().FindAsync([id], cancellationToken);
}