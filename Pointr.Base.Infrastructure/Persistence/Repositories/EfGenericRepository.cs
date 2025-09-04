using Microsoft.EntityFrameworkCore;
using Pointr.Base.Application.Interfaces;
using Pointr.Base.Infrastructure.Persistence.Specifications;

namespace Pointr.Base.Infrastructure.Persistence.Repositories;

public sealed class EfGenericRepository<TDbContext> : IGenericRepository
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EfGenericRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the first entity or null after applying query options (filter/include/sort/paging/tracking).
    /// </summary>
    public async Task<T?> FirstOrDefaultAsync<T>(
        IQueryOptions<T> queryOptions,
        CancellationToken cancellationToken
    )
        where T : class
    {
        return await QueryOptionsEvaluator
            .GetQuery(_dbContext.Set<T>().AsQueryable(), queryOptions)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Returns a single entity after applying query options; throws if zero or multiple results.
    /// </summary>
    public async Task<T> SingleAsync<T>(
        IQueryOptions<T> queryOptions,
        CancellationToken cancellationToken
    )
        where T : class
    {
        return await QueryOptionsEvaluator
            .GetQuery(_dbContext.Set<T>().AsQueryable(), queryOptions)
            .SingleAsync(cancellationToken);
    }

    /// <summary>
    /// Checks for existence after applying query options.
    /// </summary>
    public async Task<bool> AnyAsync<T>(
        IQueryOptions<T> queryOptions,
        CancellationToken cancellationToken
    )
        where T : class
    {
        return await QueryOptionsEvaluator
            .GetQuery(_dbContext.Set<T>().AsQueryable(), queryOptions)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Returns a read-only list after applying query options.
    /// </summary>
    public async Task<IReadOnlyList<T>> ListAsync<T>(
        IQueryOptions<T> queryOptions,
        CancellationToken cancellationToken
    )
        where T : class
    {
        return await QueryOptionsEvaluator
            .GetQuery(_dbContext.Set<T>().AsQueryable(), queryOptions)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds an entity to the current DbContext set.
    /// </summary>
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken)
        where T : class
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Marks an entity as modified in the current DbContext set.
    /// </summary>
    public void Update<T>(T entity)
        where T : class
    {
        _dbContext.Set<T>().Update(entity);
    }

    /// <summary>
    /// Removes an entity from the current DbContext set.
    /// </summary>
    public void Remove<T>(T entity)
        where T : class
    {
        _dbContext.Set<T>().Remove(entity);
    }
}
