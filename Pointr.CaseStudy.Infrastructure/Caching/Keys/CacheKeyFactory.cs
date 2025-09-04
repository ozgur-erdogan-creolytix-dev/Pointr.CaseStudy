using Pointr.Base.Application.Common.Cache;

namespace Pointr.CaseStudy.Infrastructure.Caching.Keys;

public static class CacheKeyFactory
{
    public static CacheKey PublishedPage(Guid siteId, string slug)
    {
        return new CacheKey("published:page", $"{siteId}:{slug}");
    }
}
