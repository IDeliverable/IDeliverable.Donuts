using System.Linq;
using System.Web.Mvc;
using IDeliverable.Donuts.Services;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.OutputCache.Services;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace IDeliverable.Donuts.Controllers
{
    [Admin]
    public class CacheItemsController : Controller
    {
        public CacheItemsController(IAuthorizer authorizer, IOutputCacheStorageProvider cacheStorageProvider, ITagCache tagCache, IItemLevelCacheService itemLevelCacheService, INotifier notifier)
        {
            mCacheStorageProvider = cacheStorageProvider;
            mTagCache = tagCache;
            mItemLevelCacheService = itemLevelCacheService;
            mNotifier = notifier;
            mAuthorizer = authorizer;
        }

        private readonly IOutputCacheStorageProvider mCacheStorageProvider;
        private readonly ITagCache mTagCache;
        private readonly IItemLevelCacheService mItemLevelCacheService;
        private readonly INotifier mNotifier;
        private readonly IAuthorizer mAuthorizer;

        public Localizer T { get; set; }

        [ActionName("View")]
        public ActionResult ViewCacheItem(string cacheKey)
        {
            return View(mItemLevelCacheService.GetCacheItem(cacheKey));
        } 

        public ActionResult EvictByKey(string cacheKey, string returnUrl)
        {
            if (!mAuthorizer.Authorize(ItemLevelCachePermissions.EvictItemLevelCacheItems, T("You do not have permission to evict item-level cache items.")))
                return new HttpUnauthorizedResult();

            mCacheStorageProvider.Remove(cacheKey);
            mNotifier.Information(T("The cache item was removed."));

            return this.RedirectLocal(returnUrl);
        }

        public ActionResult EvictByTag(string tag, string returnUrl)
        {
            if (!mAuthorizer.Authorize(ItemLevelCachePermissions.EvictItemLevelCacheItems, T("You do not have permission to evict item-level cache items.")))
                return new HttpUnauthorizedResult();

            var taggedItems = mTagCache.GetTaggedItems(tag).ToList();

            foreach (var cacheKey in taggedItems)
            {
                mCacheStorageProvider.Remove(cacheKey);
            }

            mNotifier.Information(T.Plural("There were no cache items to remove.", "1 cache item was removed.", "{0} cache items were removed.", taggedItems.Count(), taggedItems.Count()));

            return this.RedirectLocal(returnUrl);
        }
    }
}