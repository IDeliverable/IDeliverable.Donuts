using Orchard.UI.Resources;

namespace IDeliverable.Donuts
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineScript("ItemLevelCacheSettings").SetUrl("itemLevelCacheSettings.min.js", "itemLevelCacheSettings.js").SetDependencies("jQuery");
        }
    }
}
