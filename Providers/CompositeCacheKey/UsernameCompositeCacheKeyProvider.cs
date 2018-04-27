using Orchard;

namespace IDeliverable.Donuts.Providers.CompositeCacheKey
{
    public class UsernameCompositeCacheKeyProvider : ICompositeCacheKeyProvider
    {
        public UsernameCompositeCacheKeyProvider(IOrchardServices orchardServices)
        {
            mOrchardServices = orchardServices;
        }

        private readonly IOrchardServices mOrchardServices;

        public string GetCacheKeyValue()
        {
            var currentUser = mOrchardServices.WorkContext.CurrentUser;

            if (currentUser == null)
                return null;

            return currentUser.UserName;
        }

        public string Name => "User";
        public string HintText => "Cache a unique item for each user.";
        public string TechnicalName => "User";
    }
}