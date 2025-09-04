using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Pointr.Base.Application.Common.Cache;
using Pointr.Base.Application.Interfaces;

namespace Pointr.Base.Infrastructure.Caching.Memory;

public sealed class MemoryCacheClient : ICacheClient
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheClient(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(CacheKey key, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var cacheKey = key.ToString();
        _memoryCache.TryGetValue(cacheKey, out T? value);
        return Task.FromResult(value);
    }

    public async Task<T?> GetOrSetAsync<T>(
        CacheKey key,
        Func<CancellationToken, Task<T?>> factory,
        CacheOptions options,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = key.ToString();

        // Fast path: return if already cached (including negative cache if applicable)
        if (_memoryCache.TryGetValue(cacheKey, out T? cached))
            return cached;

        // Miss: compute via factory
        var result = await factory(cancellationToken);

        if (result is null)
        {
            // Negative caching: store null/default for a short TTL if configured
            if (options.NegativeTtl is not null)
                _memoryCache.Set(cacheKey, default(T), options.NegativeTtl.Value);
        }
        else
        {
            // Positive cache with Absolute TTL if provided; otherwise use default
            if (options.AbsoluteTtl is not null)
                _memoryCache.Set(cacheKey, result, options.AbsoluteTtl.Value);
            else
                _memoryCache.Set(cacheKey, result);
        }

        return result;
    }

    public Task SetAsync<T>(
        CacheKey key,
        T? value,
        CacheOptions options,
        CancellationToken cancellationToken
    )
    {
        _ = cancellationToken;

        var cacheKey = key.ToString();

        if (value is null)
        {
            // If null and NegativeTtl is set, write a negative cache entry; else remove.
            if (options.NegativeTtl is not null)
                _memoryCache.Set(cacheKey, default(T), options.NegativeTtl.Value);
            else
                _memoryCache.Remove(cacheKey);
        }
        else
        {
            if (options.AbsoluteTtl is not null)
                _memoryCache.Set(cacheKey, value, options.AbsoluteTtl.Value);
            else
                _memoryCache.Set(cacheKey, value);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(CacheKey key, CancellationToken cancellationToken)
    {
        // IMemoryCache is synchronous; cancellation token is not used here.
        _ = cancellationToken;

        var cacheKey = key.ToString();
        _memoryCache.Remove(cacheKey);
        return Task.CompletedTask;
    }
}
