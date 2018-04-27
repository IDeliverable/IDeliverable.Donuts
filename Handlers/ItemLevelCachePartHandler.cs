using System.Collections.Generic;
using System.Linq;
using IDeliverable.Donuts.Extensions;
using IDeliverable.Donuts.Helpers;
using IDeliverable.Donuts.Models;
using IDeliverable.Donuts.Models.Enums;
using IDeliverable.Donuts.Providers;
using IDeliverable.Donuts.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.OutputCache.Services;
using Orchard.Services;

namespace IDeliverable.Donuts.Handlers
{
    public class ItemLevelCachePartHandler : ContentHandler
    {
        public ItemLevelCachePartHandler(IJsonConverter jsonConverter, ICacheService cacheService, IContentManager contentManager, IShapeDisplay shapeDisplay, IItemLevelCacheService itemLevelCacheService, IEnumerable<ICompositeCacheKeyProvider> compositeCacheKeyProviders)
        {
            mCacheService = cacheService;
            mContentManager = contentManager;
            mShapeDisplay = shapeDisplay;
            mItemLevelCacheService = itemLevelCacheService;
            mCompositeCacheKeyProviderList = compositeCacheKeyProviders;

            OnInitializing<ItemLevelCachePart>((context, part) =>
            {
                // This is required to initialize the settings for new content items.
                InitializeSettings(part);
            });

            OnLoading<ItemLevelCachePart>((context, part) =>
            {
                // This is required to initialize the settings for existing content items.
                if (part.SerializedItemLevelCacheSettings != null)
                    part.ItemLevelCacheSettings = jsonConverter.Deserialize<Dictionary<string, ItemLevelCacheSettings>>(part.SerializedItemLevelCacheSettings);

                InitializeSettings(part);
            });

            OnPublished<ItemLevelCachePart>((context, part) => Invalidate(part));
        }

        private readonly ICacheService mCacheService;
        private readonly IContentManager mContentManager;
        private readonly IShapeDisplay mShapeDisplay;

        private readonly IItemLevelCacheService mItemLevelCacheService;
        private readonly IEnumerable<ICompositeCacheKeyProvider> mCompositeCacheKeyProviderList;

        private void Invalidate(ItemLevelCachePart part)
        {
            InitializeSettings(part);

            foreach (var settings in part.ItemLevelCacheSettings)
            {
                var displayType = settings.Key;

                switch (settings.Value.InvalidationAction)
                {
                    case ItemLevelCacheInvalidationAction.Evict:

                        mCacheService.RemoveByTag(ItemLevelCacheTag.For(part, displayType));
                        break;

                    case ItemLevelCacheInvalidationAction.PreRender:

                        var shape = mContentManager.BuildDisplay(part.ContentItem, displayType);
                        shape.BypassDonutTokenization = true;

                        var markup = (string)mShapeDisplay.Display(shape);
                        var cacheItem = new ItemLevelCacheItem(markup);

                        mCacheService.RemoveByTag(ItemLevelCacheTag.For(part, displayType));

                        // Give the markup to the item-level cache service which will cache it if required.
                        mItemLevelCacheService.CacheItem(part.ContentItem, displayType, cacheItem);
                        break;
                }
            }
        }

        private void InitializeSettings(ItemLevelCachePart part)
        {
            part.CompositeCacheKeyProviders = mCompositeCacheKeyProviderList.OrderBy(p => p.Name);

            // Seed the definition dictionary based on the definition of the content type (if required).
            var contentTypeSettings = part.Settings.GetModel<Dictionary<string, ItemLevelCacheSettings>>(Constants.ContentTypeDefinitionSettingsKey);
            if (contentTypeSettings != null)
            {
                part.ItemLevelCacheSettings = part.ItemLevelCacheSettings
                                                .Where(kvp => !kvp.Value.InheritDefaultSettings) // Exclude any saved settings where they shouldn't override the default settings...
                                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                                                .Merge(contentTypeSettings); // ...because the default settings will then be added in here.

                part.ContentTypeCacheSettings = contentTypeSettings;
            }

            var defaultSettings = new Dictionary<string, ItemLevelCacheSettings>()
            {
                {"Detail", new ItemLevelCacheSettings()}
            };

            // Always ensure there is at least one display type configured.
            if (!part.ItemLevelCacheSettings.Any())
                part.ItemLevelCacheSettings = defaultSettings;

            if (!part.ContentTypeCacheSettings.Any())
                part.ContentTypeCacheSettings = defaultSettings;


            PopulateCompositeCacheKeyProviders(part.ItemLevelCacheSettings);
            PopulateCompositeCacheKeyProviders(part.ContentTypeCacheSettings);
        }

        private void PopulateCompositeCacheKeyProviders(Dictionary<string, ItemLevelCacheSettings> settingsDictionary)
        {
            foreach (var settings in settingsDictionary)
            {
                // Ensure that no composite cache key providers are enabled if invalidation mode is pre-render.
                if (settings.Value.InvalidationAction == ItemLevelCacheInvalidationAction.PreRender)
                    settings.Value.CompositeCacheKeyProviders = mCompositeCacheKeyProviderList.ToDictionary(p => p.TechnicalName, p => false);
                else
                {
                    // Ensure that all composite cache key providers are in the dictionary (so that providers that have been enabled since this record was last saved are included).
                    settings.Value.CompositeCacheKeyProviders = settings.Value.CompositeCacheKeyProviders ?? new Dictionary<string, bool>();
                    settings.Value.CompositeCacheKeyProviders = settings.Value.CompositeCacheKeyProviders.Merge(mCompositeCacheKeyProviderList.ToDictionary(p => p.TechnicalName, p => false));
                }
            }
        }
    }
}