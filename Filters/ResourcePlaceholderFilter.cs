using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;

namespace IDeliverable.Donuts.Filters
{
    /// <summary>
    /// This filter is responsible for placing the resource placeholders into the correct places within the page's markup.
    /// These placeholders will be used to re-instate resources required by cached content items.
    /// </summary>
    [OrchardFeature("IDeliverable.Donuts.Resources")]
    public class ResourcePlaceholderFilter : FilterProvider, IResultFilter
    {
        public ResourcePlaceholderFilter(IWorkContextAccessor workContextAccessor, IShapeFactory shapeFactory)
        {
            mWorkContextAccessor = workContextAccessor;
            mShapeFactory = shapeFactory;
        }

        private readonly IWorkContextAccessor mWorkContextAccessor;
        private readonly dynamic mShapeFactory;

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // Should only run on a full view rendering result.
            if (!(filterContext.Result is ViewResult))
                return;

            var ctx = mWorkContextAccessor.GetContext();
            var head = ctx.Layout.Head;
            var tail = ctx.Layout.Tail;

            head.Add(mShapeFactory.ResourcePlaceholder(requestContext: filterContext.RequestContext, resourceType: "Metas"));
            head.Add(mShapeFactory.ResourcePlaceholder(requestContext: filterContext.RequestContext, resourceType: "HeadLinks"));
            head.Add(mShapeFactory.ResourcePlaceholder(requestContext: filterContext.RequestContext, resourceType: "StylesheetLinks"));
            head.Add(mShapeFactory.ResourcePlaceholder(requestContext: filterContext.RequestContext, resourceType: "HeadScripts"));
            tail.Add(mShapeFactory.ResourcePlaceholder(requestContext: filterContext.RequestContext, resourceType: "FootScripts"));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

        [Shape]
        public void ResourcePlaceholder(string resourceType, RequestContext requestContext, TextWriter Output)
        {
            // This check must be applied here as you can't guarantee that the filter will have been applied yet in OnResultExecuting.
            if (!AdminFilter.IsApplied(requestContext))
                Output.WriteLine($"%%{{ItemLevelCacheResource::{resourceType}}}");
        }
    }
}