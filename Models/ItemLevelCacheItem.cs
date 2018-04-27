using System.Collections.Generic;

namespace IDeliverable.Donuts.Models
{
    public class ItemLevelCacheItem
    {
        public ItemLevelCacheItem(string markup) : this()
        {
            Markup = markup;
        }

        public ItemLevelCacheItem()
        {
            RequiredResources = new List<RequiredResourceModel>();
            IncludedResources = new List<IncludedResourceModel>();
            FootScripts = new List<string>();
            HeadScripts = new List<string>();
        }

        public string Markup { get; set; }
        public IList<RequiredResourceModel> RequiredResources { get; set; }
        public IList<IncludedResourceModel> IncludedResources { get; set; }
        public IList<string> FootScripts { get; set; }
        public IList<string> HeadScripts { get; set; }
    }
}