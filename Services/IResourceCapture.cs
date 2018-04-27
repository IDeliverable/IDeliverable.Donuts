using IDeliverable.Donuts.Models;
using Orchard;

namespace IDeliverable.Donuts.Services
{
    public interface IResourceCapture : IDependency
    {
        void BeginCapture();
        ItemLevelCacheItem EndCapture(string markup);
    }
}
