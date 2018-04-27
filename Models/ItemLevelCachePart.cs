using System.Collections.Generic;
using IDeliverable.Donuts.Providers;
using Orchard.ContentManagement;

namespace IDeliverable.Donuts.Models
{
    public class ItemLevelCachePart : ContentPart
    {
        public ItemLevelCachePart()
        {
            ItemLevelCacheSettings = new Dictionary<string, ItemLevelCacheSettings>();
        }

        /// <summary>
        /// Always use <see href="ItemLevelCacheSettings" /> instead of this property. This property is to allow for importing and exporting only.
        /// </summary>
        internal string SerializedItemLevelCacheSettings
        {
            get { return this.Retrieve(x => x.SerializedItemLevelCacheSettings); }
            set { this.Store(x => x.SerializedItemLevelCacheSettings, value); }
        }
 
        public Dictionary<string, ItemLevelCacheSettings> ItemLevelCacheSettings { get; set; }
        public Dictionary<string, ItemLevelCacheSettings> ContentTypeCacheSettings { get; set; }
        public IEnumerable<ICompositeCacheKeyProvider> CompositeCacheKeyProviders { get; set; }
    }
}