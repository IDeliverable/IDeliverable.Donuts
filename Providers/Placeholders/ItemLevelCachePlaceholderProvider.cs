using System;
using System.Text.RegularExpressions;
using System.Threading;
using IDeliverable.Donuts.Extensions;
using IDeliverable.Donuts.Models;
using IDeliverable.Donuts.Services;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Services;
using Orchard.UI.Resources;

namespace IDeliverable.Donuts.Providers.Placeholders
{
    public class ItemLevelCachePlaceholderProvider : IPlaceholderProvider
    {
        public ItemLevelCachePlaceholderProvider(
            IContentManager contentManager, 
            IShapeDisplay shapeDisplay, 
            IItemLevelCacheService itemLevelCacheService, 
            IJsonConverter jsonConverter,
            IResourceCapture resourceCapture,
            IResourceManager resourceManager)
        {
            mContentManager = contentManager;
            mShapeDisplay = shapeDisplay;
            mItemLevelCacheService = itemLevelCacheService;
            mJsonConverter = jsonConverter;
            mResourceCapture = resourceCapture;
            mResourceManager = resourceManager;
        }

        private readonly IContentManager mContentManager;
        private readonly IShapeDisplay mShapeDisplay;
        private readonly IItemLevelCacheService mItemLevelCacheService;
        private readonly IJsonConverter mJsonConverter;
        private readonly IResourceCapture mResourceCapture;
        private readonly IResourceManager mResourceManager;

        public string ResolvePlaceholder(string placeholderText)
        {
            // Find all cache tokens.
            var matches = Regex.Matches(placeholderText, @"ItemLevelCache::(.*?)::(.*?)::(.*?)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

            if (matches.Count == 0)
                return null;

            var cacheToken = GetCacheTokenFromString(placeholderText);

            if (cacheToken == null)
                return null;

            var cacheKey = mItemLevelCacheService.GetCacheKey(cacheToken.ContentItemId, cacheToken.DisplayType, cacheToken.CacheSettings);
            var cacheItem = mItemLevelCacheService.GetCacheItem(cacheKey);

            if (cacheItem != null)
            {
                // Render the content unless another request is already doing so.
                if (cacheItem.IsInGracePeriod && Monitor.TryEnter(cacheKey))
                    return BuildDisplay(placeholderText);

                // TODO: This line is required if the resources feature is active
                // ReinstateResources(cacheItem.ItemLevelCacheItem);

                return cacheItem.ItemLevelCacheItem.Markup;
            }

            return BuildDisplay(placeholderText);
        }

        CacheToken GetCacheTokenFromString(string tokenString)
        {
            // ItemLevelCache::<ContentItemId>::<DisplayType>::<Base64EncodedSettings
            var tokenParts = tokenString.Split(new[] { "::" }, StringSplitOptions.None);
            if (tokenParts.Length < 4)
                return null;

            int contentItemId;

            if (!Int32.TryParse(tokenParts[1], out contentItemId))
                return null;

            return new CacheToken()
            {
                ContentItemId = contentItemId,
                DisplayType = tokenParts[2],
                CacheSettings = mJsonConverter.Deserialize<ItemLevelCacheSettings>(tokenParts[3].FromBase64())
            };
        }

        void ReinstateResources(ItemLevelCacheItem cacheItem)
        {
            foreach (var resource in cacheItem.IncludedResources)
                mResourceManager.Include(resource.ResourceType, resource.ResourcePath, resource.ResourceDebugPath, resource.RelativeFromPath);

            foreach (var resource in cacheItem.RequiredResources)
                mResourceManager.Require(resource.ResourceType, resource.ResourceName);

            foreach (var script in cacheItem.HeadScripts)
                mResourceManager.RegisterHeadScript(script);

            foreach (var script in cacheItem.FootScripts)
                mResourceManager.RegisterFootScript(script);
        }

        string BuildDisplay(string placeholderText)
        {
            // If the item is not in the cache, we need to build it.
            var cacheToken = GetCacheTokenFromString(placeholderText);
            if (cacheToken == null)
                return null;

            var contentItem = mContentManager.Get(cacheToken.ContentItemId);
            if (contentItem == null)
                return null;

            var shape = mContentManager.BuildDisplay(contentItem, cacheToken.DisplayType);

            shape.BypassDonutTokenization = true;

            mResourceCapture.BeginCapture();

            var markup = (string) mShapeDisplay.Display(shape);

            var cacheItem = mResourceCapture.EndCapture(markup);

            // Give the markup to the donut cache service which will cache it if required.
            mItemLevelCacheService.CacheItem(contentItem, cacheToken.DisplayType, cacheItem);

            return cacheItem.Markup;
        }
    }
}