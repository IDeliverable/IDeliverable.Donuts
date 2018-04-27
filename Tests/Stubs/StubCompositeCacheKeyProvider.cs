using IDeliverable.Donuts.Providers;

namespace IDeliverable.Donuts.Tests.Stubs
{
    public class StubCompositeCacheKeyProvider : ICompositeCacheKeyProvider
    {
        private readonly string _returns;

        public StubCompositeCacheKeyProvider(string technicalName, string returns)
        {
            _returns = returns;

            TechnicalName = technicalName;
        }

        public string GetCacheKeyValue()
        {
            return _returns;
        }

        public string Name { get; }
        public string HintText { get; }
        public string TechnicalName { get; }
    }
}
