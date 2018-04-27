using Orchard;

namespace IDeliverable.Donuts.Providers
{
    public interface ICompositeCacheKeyProvider : IDependency
    {
        string GetCacheKeyValue();
        string Name { get; }
        string HintText { get; }
        string TechnicalName { get; }
    }
}
