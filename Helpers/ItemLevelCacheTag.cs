using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;

namespace IDeliverable.Donuts.Helpers
{
    public static class ItemLevelCacheTag
    {
        public static string For(IContent contentItem)
        {
            if (contentItem == null)
                return null;

            return $"ItemLevelCache-{contentItem.Id}";
        }

        public static string For(IContent contentItem, string displayType)
        {
            if (contentItem == null)
                return null;

            return $"ItemLevelCache-{contentItem.Id}-{displayType}";
        }

        public static string For(ContentTypeDefinition contentTypeDefinition)
        {
            if (contentTypeDefinition == null)
                return null;

            return $"ItemLevelCache-ContentType-{contentTypeDefinition.Name}";
        }

        public static string GenericTag => "ItemLevelCacheItem";
    }

    public static class PageLevelCacheTag
    {
        // All of these values are magic ints, which sucks, but is required as Orchard's output cache only allows you to contribute ints to the tag cache.
        public static int? For(IContent contentItem)
        {
            if (contentItem == null)
                return null;

            return -contentItem.Id;
        }

        public static int GenericTag => 0;
    }

}