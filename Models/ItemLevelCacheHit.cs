namespace IDeliverable.Donuts.Models
{
    public class ItemLevelCacheHit
    {
        public CacheItemModel CacheItem { get; set; }
        public ItemLevelCacheItem ItemLevelCacheItem { get; set; }
        public bool IsInGracePeriod { get; set; }
    }
}