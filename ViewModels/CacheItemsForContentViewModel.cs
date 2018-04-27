using System.Collections.Generic;
using IDeliverable.Donuts.Models;

namespace IDeliverable.Donuts.ViewModels
{
    public class CacheItemsForContentViewModel
    {
        public bool UserCanEvict { get; set; }
        public string Tag { get; set; }
        public IEnumerable<CacheItemModel> CacheItems { get; set; }
    }
}