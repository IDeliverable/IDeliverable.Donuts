using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using IDeliverable.Donuts.Extensions;
using IDeliverable.Donuts.Helpers;
using IDeliverable.Donuts.Models;
using IDeliverable.Donuts.Models.Enums;
using IDeliverable.Donuts.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.OutputCache.Services;
using Orchard.Security;
using Orchard.Services;
using Orchard.UI.Notify;

namespace IDeliverable.Donuts.Drivers
{
    public class ItemLevelCachePartDriver : ContentPartDriver<ItemLevelCachePart>
    {
        public ItemLevelCachePartDriver(
            IAuthorizer authorizer, 
            IOutputCacheStorageProvider outputCacheStorageProvider, 
            IJsonConverter jsonConverter, 
            INotifier notifier,
            ITagCache tagCache,
            UrlHelper urlHelper)
        {
            mAuthorizer = authorizer;
            mOutputCacheStorageProvider = outputCacheStorageProvider;
            mJsonConverter = jsonConverter;
            mNotifier = notifier;
            mTagCache = tagCache;
            mUrlHelper = urlHelper;

            T = NullLocalizer.Instance;
        }

        private readonly IAuthorizer mAuthorizer;
        private readonly IOutputCacheStorageProvider mOutputCacheStorageProvider;
        private readonly IJsonConverter mJsonConverter;
        private readonly INotifier mNotifier;
        private readonly ITagCache mTagCache;
        private readonly UrlHelper mUrlHelper;

        public Localizer T { get; set; }

        protected override DriverResult Editor(ItemLevelCachePart part, dynamic shapeHelper)
        {
            var driverResults = new List<DriverResult>();
            
            if (mAuthorizer.Authorize(ItemLevelCachePermissions.EditItemLevelCacheSettings))
                driverResults.Add(ContentShape("Parts_ItemLevelCacheSettings_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ItemLevelCacheSettings", Model: part, Prefix: Prefix)));

            if (mAuthorizer.Authorize(ItemLevelCachePermissions.ViewItemLevelCacheItems))
            {
                driverResults.Add(ContentShape("Parts_ItemLevelCache_CacheItemsForContent", () =>
                {
                    var outputCacheItems = mOutputCacheStorageProvider.GetCacheItems(0, mOutputCacheStorageProvider.GetCacheItemsCount()).Where(i => i.Tags.Contains(ItemLevelCacheTag.For(part)));
                    var vm = new CacheItemsForContentViewModel()
                    {
                        UserCanEvict = mAuthorizer.Authorize(ItemLevelCachePermissions.EvictItemLevelCacheItems),
                        Tag = ItemLevelCacheTag.For(part),
                        // TODO: Would it be more efficient to get all the cache keys using ITagCache, as opposed to getting all then filtering?
                        CacheItems = outputCacheItems.Select(ci => ci.ToCacheItemModel())
                    };

                    return shapeHelper.EditorTemplate(TemplateName: "Parts/CacheItemsForContent", Model: vm, Prefix: Prefix);
                }));
            }

            return Combined(driverResults.ToArray());
        }

        protected override DriverResult Editor(ItemLevelCachePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (!mAuthorizer.Authorize(ItemLevelCachePermissions.EditItemLevelCacheSettings))
                return null;

            updater.TryUpdateModel(part, Prefix, null, null);

            part.SerializedItemLevelCacheSettings = mJsonConverter.Serialize(part.ItemLevelCacheSettings);

            if (mTagCache.GetTaggedItems(PageLevelCacheTag.For(part).ToString()).Any())
            {
                var contentType = part.ContentItem.TypeDefinition.DisplayName;
                var returnUrl = mUrlHelper.Action("Index", new
                {
                    area = "Orchard.Widgets",
                    controller = "Admin"
                });

                var url = mUrlHelper.Action("EvictByTag", new
                {
                    area = "IDeliverable.Donuts",
                    controller = "CacheItems",
                    tag = PageLevelCacheTag.For(part),
                    returnUrl
                });

                mNotifier.Warning(
                    T("There are some page level cache items that contain the {0} you have just edited. " +
                        "Any new item level cache settings you have applied to this item will not apply on those pages until their page level cache items expire. " +
                        "You can <a href=\"{1}\">evict these items now</a> in order for the changes to take effect immediately.", contentType, url));
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(ItemLevelCachePart part, ExportContentContext context)
        {
            var elements = new List<XElement>();

            // Filter all the saved settings so that only overrides are exported, as a fail safe
            foreach (var setting in part.ItemLevelCacheSettings.Where(x => !x.Value.InheritDefaultSettings))
            {
                var element = new XElement(setting.Key)
                                    .Attr("Mode", setting.Value.Mode)
                                    .Attr("InvalidationAction", setting.Value.InvalidationAction)
                                    .Attr("CacheDurationSeconds", setting.Value.CacheDurationSeconds)
                                    .Attr("CacheGraceTimeSeconds", setting.Value.CacheGraceTimeSeconds);

                var compositeCacheKeyElement = new XElement("CompositeCacheKeys");

                foreach (var compositeCacheKeyProvider in setting.Value.CompositeCacheKeyProviders)
                {
                    compositeCacheKeyElement.Add(new XElement(compositeCacheKeyProvider.Key).Attr("Enabled", compositeCacheKeyProvider.Value));
                }

                element.Add(compositeCacheKeyElement);

                elements.Add(element);
            }

            context.Element(part.PartDefinition.Name).Add(elements);
        }

        protected override void Importing(ItemLevelCachePart part, ImportContentContext context)
        {
            var element = context.Data.Element(part.PartDefinition.Name);

            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null)
                return;

            var dictionary = new Dictionary<string, ItemLevelCacheSettings>();

            foreach (var displayTypeElement in element.Elements())
            {
                var settings = new ItemLevelCacheSettings
                {
                    InheritDefaultSettings = false,
                    Mode = (ItemLevelCacheMode)Enum.Parse(typeof(ItemLevelCacheMode), displayTypeElement.Attribute("Mode").Value),
                    InvalidationAction = (ItemLevelCacheInvalidationAction)Enum.Parse(typeof(ItemLevelCacheInvalidationAction), displayTypeElement.Attribute("InvalidationAction").Value),
                    CacheDurationSeconds = (int)displayTypeElement.Attribute("CacheDurationSeconds"),
                    CacheGraceTimeSeconds = (int)displayTypeElement.Attribute("CacheGraceTimeSeconds")
                };

                var compositeCacheKeysElement = displayTypeElement.Element("CompositeCacheKeys");

                if (compositeCacheKeysElement != null)
                {
                    foreach (var compositeCacheKeyElement in compositeCacheKeysElement.Elements())
                    {
                        settings.CompositeCacheKeyProviders[compositeCacheKeyElement.Name.LocalName] = (bool)compositeCacheKeyElement.Attribute("Enabled");
                    }
                }

                dictionary[displayTypeElement.Name.LocalName] = settings;
            }

            part.ItemLevelCacheSettings = dictionary;
            part.SerializedItemLevelCacheSettings = mJsonConverter.Serialize(part.ItemLevelCacheSettings);
        }
    }
}