using System.Collections.Generic;
using IDeliverable.Donuts.Models.Enums;

namespace IDeliverable.Donuts.Models
{
    public class ItemLevelCacheSettings
    {
        public ItemLevelCacheSettings()
        {
            InheritDefaultSettings = true;
            CompositeCacheKeyProviders = new Dictionary<string, bool>();
        }
        
        public bool InheritDefaultSettings { get; set; }

        public ItemLevelCacheMode Mode { get; set; }
        public ItemLevelCacheInvalidationAction InvalidationAction { get; set; }
        public int CacheDurationSeconds { get; set; }
        public int CacheGraceTimeSeconds { get; set; }
        public Dictionary<string, bool> CompositeCacheKeyProviders { get; set; }
    }
}