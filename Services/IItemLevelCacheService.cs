using IDeliverable.Donuts.Models;
using Orchard;
using Orchard.ContentManagement;

namespace IDeliverable.Donuts.Services
{
    public interface IItemLevelCacheService : IDependency
    {
        bool MarkupShouldBeTokenized(IContent contentItem, string displayType);
        string GetPlaceholderText(IContent contentItem, string displayType);
        string GetCacheKey(IContent contentItem, string displayType);
        string GetCacheKey(int contentItemId, string displayType, ItemLevelCacheSettings cacheSettings);
        ItemLevelCacheHit GetCacheItem(string cacheKey);
        void CacheItem(IContent contentItem, string displayType, ItemLevelCacheItem itemLevelCacheItem);
    }
}