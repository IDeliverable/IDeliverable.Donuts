using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace IDeliverable.Donuts
{
    public class ItemLevelCachePermissions : IPermissionProvider
    {
        public static readonly Permission EditItemLevelCacheSettings = new Permission { Description = "Edit item-level cache settings", Name = nameof(EditItemLevelCacheSettings) };
        public static readonly Permission EvictItemLevelCacheItems = new Permission { Description = "Evict cached content items", Name = nameof(EvictItemLevelCacheItems) };
        public static readonly Permission ViewItemLevelCacheItems = new Permission { Description = "View cached content items", Name = nameof(ViewItemLevelCacheItems), ImpliedBy = new[] { EvictItemLevelCacheItems } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] 
            {
                EditItemLevelCacheSettings,
                EvictItemLevelCacheItems,
                ViewItemLevelCacheItems,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] 
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] 
                    {
                        EditItemLevelCacheSettings,
                        EvictItemLevelCacheItems,
                        ViewItemLevelCacheItems,
                    }
                },
            };
        }

    }
}