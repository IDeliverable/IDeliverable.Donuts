namespace IDeliverable.Donuts.Models
{
    public class CacheToken
    {
        public int ContentItemId { get; set; }
        public string DisplayType { get; set; }
        public ItemLevelCacheSettings CacheSettings { get; set; }
    }
}