using System.Collections.Generic;
using System.Linq;
using IDeliverable.Donuts.Helpers;
using IDeliverable.Donuts.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Services;

namespace IDeliverable.Donuts.Handlers
{
    /// <summary>
    /// Saves references to content items which have been displayed during a request, excluding tokenized content.
    /// </summary>
    [OrchardSuppressDependency("Orchard.OutputCache.Handlers.DisplayedContentItemHandler")]
    public class DisplayedContentItemHandler : ContentHandler, IDisplayedContentItemHandler
    {
        public DisplayedContentItemHandler(IItemLevelCacheService itemLevelCacheService)
        {
            mItemLevelCacheService = itemLevelCacheService;

            mItemIdList = new List<int>(new [] { PageLevelCacheTag.GenericTag });
        }

        private readonly List<int> mItemIdList;
        private readonly IItemLevelCacheService mItemLevelCacheService;

        protected override void BuildDisplayShape(BuildDisplayContext context)
        {
            var contentItemTag = PageLevelCacheTag.For(context.Content);

            if (contentItemTag.HasValue)
                mItemIdList.Add(contentItemTag.Value);

            if (!mItemLevelCacheService.MarkupShouldBeTokenized(context.Content, context.DisplayType))
                mItemIdList.Add(context.Content.Id);
        }

        public bool IsDisplayed(int id)
        {
            return mItemIdList.Contains(id);
        }

        public IEnumerable<int> GetDisplayed()
        {
            return mItemIdList.Distinct();
        }
    }
}