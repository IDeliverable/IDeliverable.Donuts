using System.Collections.Generic;
using System.Web;
using IDeliverable.Donuts.Services;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Mvc;
using Orchard.UI.Admin;

namespace IDeliverable.Donuts.ShapeTableProviders
{
    public class PlaceholderSubstitutionShapeTableService : IPlaceholderSubstitutionShapeTableService
    {
        public PlaceholderSubstitutionShapeTableService(IItemLevelCacheService itemLevelCacheService, IHttpContextAccessor httpContextAccessor)
        {
            mItemLevelCacheService = itemLevelCacheService;
            mHttpContextAccessor = httpContextAccessor;
        }

        private readonly IItemLevelCacheService mItemLevelCacheService;
        private readonly IHttpContextAccessor mHttpContextAccessor;

        public void OnDisplaying(ShapeDisplayingContext context)
        {
            // Check to see if the markup for this shape is already in the cache, and if so, there is no need to render it! :)
            // We can prevent the render by setting context.ChildContent to anything other than null (the default display manager checks this property before calling render).
            // In our case, we want to set this to the placeholder value, so that it can be replaced with the cached markup later on in the request pipeline.

            if (AdminFilter.IsApplied(mHttpContextAccessor.Current().Request.RequestContext))
                return;

            var contentItem = (IContent)context.Shape.ContentItem;

            var itemLevelTokenizationShouldBeBypassed = ItemLevelTokenizationShouldBeBypassed(context);
            var markupMustNotBeTokenized = !mItemLevelCacheService.MarkupShouldBeTokenized(contentItem, context.ShapeMetadata.DisplayType);

            if (itemLevelTokenizationShouldBeBypassed || markupMustNotBeTokenized)
                return;
            
            var placeholderText = mItemLevelCacheService.GetPlaceholderText(contentItem, context.ShapeMetadata.DisplayType);

            context.ShapeMetadata.Wrappers = new List<string>(); // TODO: add in ability to toggle debug info wrappers?
            context.ChildContent = new HtmlString(WrapPlaceholderText(placeholderText));
        }

        private bool ItemLevelTokenizationShouldBeBypassed(ShapeDisplayingContext context)
        {
            try
            {
                return context.Shape.BypassDonutTokenization;
            }
            catch (RuntimeBinderException)
            {
                return false;
            }
        }

        private string WrapPlaceholderText(string placeholderText) => $"%%{{{placeholderText}}}";
    }
}