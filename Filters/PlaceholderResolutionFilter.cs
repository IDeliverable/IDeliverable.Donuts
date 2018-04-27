using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using IDeliverable.Donuts.Helpers;
using IDeliverable.Donuts.Services;
using Orchard;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;

namespace IDeliverable.Donuts.Filters
{
    public class PlaceholderResolutionFilter : FilterProvider, IResultFilter
    {
        public PlaceholderResolutionFilter(IWorkContextAccessor workContextAccessor, RequestContext requestContext)
        {
            mWorkContextAccessor = workContextAccessor;
            _requestContext = requestContext;
        }

        private readonly IWorkContextAccessor mWorkContextAccessor;
        private readonly RequestContext _requestContext;

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // This filter is not reentrant (multiple executions within the same request are
            // not supported) so child actions are ignored completely.
            if (filterContext.IsChildAction)
                return;

            if (AdminFilter.IsApplied(filterContext.RequestContext))
                return;

            var workContext = mWorkContextAccessor.GetContext();

            var currentCulture = workContext.CurrentCulture;
            var currentSite = workContext.CurrentSite;
            var currentUser = workContext.CurrentUser;
            var currentCalendar = workContext.CurrentCalendar;
            var currentTheme = workContext.CurrentTheme;
            var currentTimeZone = workContext.CurrentTimeZone;

            var response = filterContext.HttpContext.Response;
            var captureStream = new PlaceholderStream(response.Filter);
            response.Filter = captureStream;
            captureStream.TransformStream += stream =>
            {
                using (var scope = mWorkContextAccessor.CreateWorkContextScope(_requestContext.HttpContext))
                {
                    scope.WorkContext.CurrentCulture = currentCulture;
                    scope.WorkContext.CurrentSite = currentSite;
                    scope.WorkContext.CurrentUser = currentUser;
                    scope.WorkContext.CurrentCalendar = currentCalendar;
                    scope.WorkContext.CurrentTheme = currentTheme;
                    scope.WorkContext.CurrentTimeZone = currentTimeZone;

                    var html = filterContext.HttpContext.Request.ContentEncoding.GetString(stream.ToArray());
                    html = scope.Resolve<IPlaceholderService>().ResolvePlaceholders(html);

                    var buffer = filterContext.HttpContext.Request.ContentEncoding.GetBytes(html);

                    return new MemoryStream(buffer);
                };
            };
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }
    }
}