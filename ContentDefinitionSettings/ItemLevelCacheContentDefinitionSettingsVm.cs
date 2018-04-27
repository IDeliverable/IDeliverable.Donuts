using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IDeliverable.Donuts.Models;
using IDeliverable.Donuts.Providers;

namespace IDeliverable.Donuts.ContentDefinitionSettings
{
    public class ItemLevelCacheContentDefinitionSettingsVm
    {
        public Dictionary<string, ItemLevelCacheSettings> Settings { get; set; }
        public IEnumerable<ICompositeCacheKeyProvider> CompositeCacheKeyProviders { get; set; }

        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "You can only use letters or numbers for the new display type name.")]
        public string NewDisplayType { get; set; }
        public string ContentTypeDisplayName { get; set; }
    }
}