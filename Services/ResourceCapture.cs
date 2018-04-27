using IDeliverable.Donuts.Events;
using IDeliverable.Donuts.Models;

namespace IDeliverable.Donuts.Services
{
    public class ResourceCapture : IResourceCapture, IResourceManagerEvents
    {
        public void BeginCapture()
        {
            mCacheItem = new ItemLevelCacheItem();
        }

        private ItemLevelCacheItem mCacheItem { get; set; }

        public ItemLevelCacheItem EndCapture(string markup)
        {
            var cacheItem = mCacheItem ?? new ItemLevelCacheItem();
            cacheItem.Markup = markup;

            mCacheItem = null;

            return cacheItem;
        }

        public void FootScriptRegistered(string script)
        {
            if (mCacheItem == null)
                return;

            mCacheItem.FootScripts.Add(script);
        }

        public void HeadScriptRegistered(string script)
        {
            if (mCacheItem == null)
                return;

            mCacheItem.HeadScripts.Add(script);
        }

        public void ResourceRequired(string resourceType, string resourceName)
        {
            if (mCacheItem == null)
                return;

            mCacheItem.RequiredResources.Add(new RequiredResourceModel
            {
                ResourceType = resourceType,
                ResourceName = resourceName
            });
        }

        public void ResourceIncluded(string resourceType, string resourcePath, string resourceDebugPath, string relativeFromPath)
        {
            if (mCacheItem == null)
                return;

            mCacheItem.IncludedResources.Add(new IncludedResourceModel
            {
                ResourceType = resourceType,
                ResourcePath = resourcePath,
                ResourceDebugPath = resourceDebugPath,
                RelativeFromPath = relativeFromPath
            });
        }
    }
}