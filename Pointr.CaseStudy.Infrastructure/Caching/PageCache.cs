using Microsoft.EntityFrameworkCore;
using Pointr.Base.Application.Common.Cache;
using Pointr.Base.Application.Interfaces;
using Pointr.CaseStudy.Application.Interfaces;
using Pointr.CaseStudy.Domain.Entities;
using Pointr.CaseStudy.Infrastructure.Caching.Keys;
using Pointr.CaseStudy.Infrastructure.Persistence.Database;

namespace Pointr.CaseStudy.Infrastructure.Caching;

public sealed class PageCache(ICacheClient cacheClient, AppDbContext dbContext) : IPageCache
{
    // Cache options: short TTL for hits and misses
    private static readonly CacheOptions HitOptions = new()
    {
        AbsoluteTtl = TimeSpan.FromSeconds(60),
    }; // Cached page lives for 60s
    private static readonly CacheOptions MissOptions = new()
    {
        NegativeTtl = TimeSpan.FromSeconds(10),
    }; // Negative result cached for 10s

    public Task<Page?> GetPublishedPageAsync(
        Guid siteId,
        string slug,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = new CacheKey("published:page", $"{siteId}:{slug}");

        // Retrieve published page from cache, fallback to database query if not cached
        return cacheClient.GetOrSetAsync<Page>(
            cacheKey,
            async cacheEntry =>
            {
                return await dbContext
                    .Pages.AsNoTracking()
                    .FirstOrDefaultAsync(
                        page => page.SiteId == siteId && page.Slug == slug,
                        cacheEntry
                    );
            },
            new CacheOptions
            {
                AbsoluteTtl = HitOptions.AbsoluteTtl,
                NegativeTtl = MissOptions.NegativeTtl,
            },
            cancellationToken
        );
    }

    public Task Invalidate(Guid siteId, string slug)
    {
        var cacheKey = new CacheKey("published:page", $"{siteId}:{slug}");

        // Remove published page entry from cache
        return cacheClient.RemoveAsync(cacheKey, CancellationToken.None);
    }
}
