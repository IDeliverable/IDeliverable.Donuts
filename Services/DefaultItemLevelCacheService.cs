using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Donuts.Extensions;
using IDeliverable.Donuts.Helpers;
using IDeliverable.Donuts.Models;
using IDeliverable.Donuts.Models.Enums;
using IDeliverable.Donuts.Providers;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Mvc;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.Services;

namespace IDeliverable.Donuts.Services
{
    public class DefaultItemLevelCacheService : IItemLevelCacheService
    {
        public DefaultItemLevelCacheService(
            IOutputCacheStorageProvider outputCacheStorageProvider, 
            IClock clock, 
            ShellSettings shellSettings, 
            ITagCache tagCache, 
            IEnumerable<ICompositeCacheKeyProvider> compositeCacheKeyProviders, 
            IHttpContextAccessor httpContextAccessor,
            IJsonConverter jsonConverter)
        {
            mOutputCacheStorageProvider = outputCacheStorageProvider;
            mClock = clock;
            mShellSettings = shellSettings;
            mTagCache = tagCache;
            mCompositeCacheKeyProviders = compositeCacheKeyProviders;
            mHttpContextAccessor = httpContextAccessor;
            mJsonConverter = jsonConverter;
        }

        private readonly IOutputCacheStorageProvider mOutputCacheStorageProvider;
        private readonly IClock mClock;
        private readonly ShellSettings mShellSettings;
        private readonly ITagCache mTagCache;
        private readonly IEnumerable<ICompositeCacheKeyProvider> mCompositeCacheKeyProviders;
        private readonly IHttpContextAccessor mHttpContextAccessor;
        private readonly IJsonConverter mJsonConverter;

        public void CacheItem(IContent contentItem, string displayType, ItemLevelCacheItem itemLevelCacheItem)
        {
            if (contentItem == null)
                return;

            var itemLevelCachePart = contentItem.As<ItemLevelCachePart>();
            if (itemLevelCachePart == null || !itemLevelCachePart.ItemLevelCacheSettings.ContainsKey(displayType) || itemLevelCachePart.ItemLevelCacheSettings[displayType].Mode != ItemLevelCacheMode.CacheItem)
                return;

            var settings = itemLevelCachePart.ItemLevelCacheSettings[displayType];
            var cacheKey = GetCacheKey(contentItem, displayType);

            var cachedItem = mOutputCacheStorageProvider.GetCacheItem(cacheKey);
            if (cachedItem == null || cachedItem.IsInGracePeriod(mClock.UtcNow)) // TODO: Should this method check the cache first, or just blindly insert?
            {
                var serializedCacheItem = mJsonConverter.Serialize(itemLevelCacheItem);

                var cacheItem = new CacheItem()
                {
                    CachedOnUtc = mClock.UtcNow,
                    Duration = settings.CacheDurationSeconds,
                    GraceTime = settings.CacheGraceTimeSeconds,
                    Output = mHttpContextAccessor.Current().Request.ContentEncoding.GetBytes(serializedCacheItem),
                    Tags = new[]
                    {
                        ItemLevelCacheTag.GenericTag,
                        ItemLevelCacheTag.For(contentItem),
                        ItemLevelCacheTag.For(contentItem, displayType),
                        ItemLevelCacheTag.For(contentItem.ContentItem.TypeDefinition)
                    },
                    Tenant = mShellSettings.Name,
                    CacheKey = cacheKey,
                    InvariantCacheKey = cacheKey,
                    Url = ""
                };

                mOutputCacheStorageProvider.Set(cacheKey, cacheItem);

                // Also add the item tags to the tag cache.
                foreach (var tag in cacheItem.Tags)
                {
                    mTagCache.Tag(tag, cacheKey);
                }
            }
        }

        public string GetPlaceholderText(IContent contentItem, string displayType)
        {
            var cacheSettings = GetCacheSettingsFromContentItem(contentItem, displayType);

            if (cacheSettings == null)
                return null;

            return $"ItemLevelCache::{contentItem.Id}::{displayType}::{mJsonConverter.Serialize(cacheSettings).ToBase64()}";
        }

        public string GetCacheKey(IContent contentItem, string displayType)
        {
            if (contentItem == null)
                return null;

            var cacheSettings = GetCacheSettingsFromContentItem(contentItem, displayType);

            return GetCacheKey(contentItem.Id, displayType, cacheSettings);
        }

        public string GetCacheKey(int contentItemId, string displayType, ItemLevelCacheSettings cacheSettings)
        {
            if (cacheSettings == null)
                return null;

            var enabledCacheKeys = cacheSettings.CompositeCacheKeyProviders.Where(kvp => kvp.Value).Select(kvp => kvp.Key);

            var compositeCacheKeys =
                mCompositeCacheKeyProviders
                    .Where(p => enabledCacheKeys.Contains(p.TechnicalName))
                    .OrderBy(p => p.TechnicalName)
                    .Select(p => $"{p.TechnicalName}={p.GetCacheKeyValue()}");

            return $"ItemLevelCache; {contentItemId}; {displayType}; {String.Join("; ", compositeCacheKeys)}";
        }

        public bool MarkupShouldBeTokenized(IContent contentItem, string displayType)
        {
            if (contentItem == null)
                return false;

            var itemLevelCachePart = contentItem.As<ItemLevelCachePart>();
            if (itemLevelCachePart == null)
                return false;

            if (!itemLevelCachePart.ItemLevelCacheSettings.ContainsKey(displayType))
                return false;
            
            return itemLevelCachePart.ItemLevelCacheSettings[displayType].Mode != ItemLevelCacheMode.DoNothing;
        }

        public ItemLevelCacheHit GetCacheItem(string cacheKey)
        {
            // Check if the item is in the cache.
            var cacheItem = mOutputCacheStorageProvider.GetCacheItem(cacheKey);

            if (cacheItem?.Output == null)
                return null;

            var serializedItem = mHttpContextAccessor.Current().Request.ContentEncoding.GetString(cacheItem.Output);
            var itemLevelCacheItem = mJsonConverter.Deserialize<ItemLevelCacheItem>(serializedItem);

            if (itemLevelCacheItem == null)
                return null;

            return new ItemLevelCacheHit()
            {
                CacheItem = cacheItem.ToCacheItemModel(),
                ItemLevelCacheItem = itemLevelCacheItem,
                IsInGracePeriod = cacheItem.IsInGracePeriod(mClock.UtcNow)
            };
        }

        ItemLevelCacheSettings GetCacheSettingsFromContentItem(IContent contentItem, string displayType)
        {
            if (contentItem == null)
                return null;

            var itemLevelCachePart = contentItem.As<ItemLevelCachePart>();
            if (itemLevelCachePart == null || !itemLevelCachePart.ItemLevelCacheSettings.ContainsKey(displayType))
                return null;

            return itemLevelCachePart.ItemLevelCacheSettings[displayType];
        }
    }
}