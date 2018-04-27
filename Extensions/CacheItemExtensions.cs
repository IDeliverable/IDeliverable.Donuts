using IDeliverable.Donuts.Models;
using Orchard.OutputCache.Models;

namespace IDeliverable.Donuts.Extensions
{
    public static class CacheItemExtensions
    {
        public static CacheItemModel ToCacheItemModel(this CacheItem cacheItem)
        {
            return new CacheItemModel
            {
                CachedOnUtc = cacheItem.CachedOnUtc,
                Duration = cacheItem.Duration,
                GraceTime = cacheItem.GraceTime,
                Output = cacheItem.Output,
                ContentType = cacheItem.ContentType,
                QueryString = cacheItem.QueryString,
                CacheKey = cacheItem.CacheKey,
                InvariantCacheKey = cacheItem.InvariantCacheKey,
                Url = cacheItem.Url,
                Tenant = cacheItem.Tenant,
                StatusCode = cacheItem.StatusCode,
                Tags = cacheItem.Tags 
            };
        }
    }
}