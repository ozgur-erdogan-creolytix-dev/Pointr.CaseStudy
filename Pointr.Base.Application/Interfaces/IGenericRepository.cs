using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Application.Interfaces;

public interface IGenericRepository
{
    // READ operations
    Task<T?> FirstOrDefaultAsync<T>(
        IQueryOptions<T> queryOptions,
        CancellationToken cancellationToken
    )
        where T : class;

    Task<T> SingleAsync<T>(IQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        where T : class;

    Task<bool> AnyAsync<T>(IQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        where T : class;

    Task<IReadOnlyList<T>> ListAsync<T>(
        IQueryOptions<T> queryOptions,
        CancellationToken cancellationToken
    )
        where T : class;

    // WRITE operations
    Task AddAsync<T>(T entity, CancellationToken cancellationToken)
        where T : class;

    void Update<T>(T entity)
        where T : class;

    void Remove<T>(T entity)
        where T : class;
}
