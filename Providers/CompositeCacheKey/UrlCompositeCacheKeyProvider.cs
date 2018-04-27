using System.Web;
using Orchard.Mvc;

namespace IDeliverable.Donuts.Providers.CompositeCacheKey
{
    public class UrlCompositeCacheKeyProvider : ICompositeCacheKeyProvider
    {
        public UrlCompositeCacheKeyProvider(IHttpContextAccessor httpContextAccessor)
        {
            mHttpContextAccessor = httpContextAccessor;
        }

        private readonly IHttpContextAccessor mHttpContextAccessor;

        public string GetCacheKeyValue()
        {
            var httpContext = mHttpContextAccessor.Current();

            if (httpContext == null)
                return null;

            var request = httpContext.Request;

            if (request == null)
                return null;

            return VirtualPathUtility.ToAppRelative(request.Path.ToLower());
        }

        public string Name => "Request URL";
        public string HintText => "Cache a unique item for each URL.";
        public string TechnicalName => "Url";
    }
}