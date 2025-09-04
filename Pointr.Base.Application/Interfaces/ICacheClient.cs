using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pointr.Base.Application.Common.Cache;

namespace Pointr.Base.Application.Interfaces;

public interface ICacheClient
{
    Task<T?> GetAsync<T>(CacheKey cacheKey, CancellationToken cancellationToken);
    Task<T?> GetOrSetAsync<T>(
        CacheKey cacheKey,
        Func<CancellationToken, Task<T?>> factory,
        CacheOptions options,
        CancellationToken cancellationToken
    );
    Task SetAsync<T>(
        CacheKey cacheKey,
        T? value,
        CacheOptions options,
        CancellationToken cancellationToken
    );
    Task RemoveAsync(CacheKey cacheKey, CancellationToken cancellationToken);
}
