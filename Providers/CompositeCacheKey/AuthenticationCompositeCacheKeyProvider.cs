using Orchard;

namespace IDeliverable.Donuts.Providers.CompositeCacheKey
{
    public class AuthenticationCompositeCacheKeyProvider : ICompositeCacheKeyProvider
    {
        public AuthenticationCompositeCacheKeyProvider(IOrchardServices orchardServices)
        {
            mOrchardServices = orchardServices;
        }

        private readonly IOrchardServices mOrchardServices;

        public string GetCacheKeyValue()
        {
            return (mOrchardServices.WorkContext.CurrentUser != null).ToString();
        }

        public string Name => "Authentication State";
        public string HintText => "Cache a unique item for authenticated versus unauthenticated requests.";
        public string TechnicalName => "Auth";
    }
}