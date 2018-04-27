using Orchard;

namespace IDeliverable.Donuts.Providers.CompositeCacheKey
{
    public class CultureCompositeCacheKeyProvider : ICompositeCacheKeyProvider
    {
        public CultureCompositeCacheKeyProvider(IOrchardServices orchardServices)
        {
            mOrchardServices = orchardServices;
        }

        private readonly IOrchardServices mOrchardServices;

        public string GetCacheKeyValue()
        {
            return mOrchardServices.WorkContext.CurrentCulture;
        }

        public string Name => "Request Culture";
        public string HintText => "Cache a unique item for each enabled culture.";
        public string TechnicalName => "Culture";
    }
}