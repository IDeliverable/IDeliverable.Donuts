using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDeliverable.Donuts.Helpers;
using IDeliverable.Donuts.Models;
using IDeliverable.Donuts.Models.Enums;
using IDeliverable.Donuts.Providers;
using IDeliverable.Donuts.Services;
using IDeliverable.Donuts.Tests.Stubs;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.Services;
using Orchard.Tests.Stubs;

namespace IDeliverable.Donuts.Tests.Services
{
    [TestFixture]
    public class DefaultDonutCacheServiceTests
    {
        private DefaultItemLevelCacheService mSut;

        private Mock<IOutputCacheStorageProvider> mOutputCacheStorageProvider;
        private Mock<IClock> mClock;
        private Mock<ITagCache> _tagCache;

        private IList<ICompositeCacheKeyProvider> mCompositeCacheKeyProviders;
        private ShellSettings mShellSettings;
        private DefaultJsonConverter mJsonConvertor;

        private Dictionary<string, CacheItem> mCachedItems;

        [SetUp]
        public void Setup()
        {
            mOutputCacheStorageProvider = new Mock<IOutputCacheStorageProvider>();
            mClock = new Mock<IClock>();
            _tagCache = new Mock<ITagCache>();

            // Some tests create their own collection of providers, so only create a new instance if this is not the case
            if (mCompositeCacheKeyProviders == null)
            {
                mCompositeCacheKeyProviders = new List<ICompositeCacheKeyProvider>();
            }

            mCachedItems = new Dictionary<string, CacheItem>();

            mShellSettings = new ShellSettings
            {
                Name = "UnitTest"
            };

            mJsonConvertor = new DefaultJsonConverter();

            mOutputCacheStorageProvider.Setup(p => p.GetCacheItem("miss")).Returns(null as CacheItem);
            mOutputCacheStorageProvider.Setup(p => p.GetCacheItem("hit")).Returns(new CacheItem
            {
                Output = Encoding.UTF8.GetBytes(mJsonConvertor.Serialize(new ItemLevelCacheItem("html")))
            });

            mOutputCacheStorageProvider.Setup(p => p.GetCacheItem("ItemLevelCache; 42; AlwaysHit; ")).Returns(new CacheItem());
            mOutputCacheStorageProvider.Setup(p => p.GetCacheItem("ItemLevelCache; 42; AlwaysMiss; ")).Returns(null as CacheItem);

            mOutputCacheStorageProvider.Setup(p => p.Set(It.IsAny<string>(), It.IsAny<CacheItem>())).Callback<string, CacheItem>((key, item) => mCachedItems[key] = item);

            mClock.Setup(c => c.UtcNow).Returns(new DateTime(2016, 10, 26));

            mSut = new DefaultItemLevelCacheService(mOutputCacheStorageProvider.Object,
                                                    mClock.Object,
                                                    mShellSettings,
                                                    _tagCache.Object,
                                                    mCompositeCacheKeyProviders,
                                                    new StubHttpContextAccessor(new DonutCacheStubHttpContext()),
                                                    mJsonConvertor);
        }

        [Test]
        public void CacheItem_ReturnsWhenContentItemIsNull()
        {
            mSut.CacheItem(null, "Detail", new ItemLevelCacheItem("html"));

            mOutputCacheStorageProvider.Verify(p => p.GetCacheItem(It.IsAny<string>()), Times.Never());
            mOutputCacheStorageProvider.Verify(p => p.Set(It.IsAny<string>(), It.IsAny<CacheItem>()), Times.Never());
        }

        [Test]
        public void CacheItem_ReturnsWhenDonutCachePartIsNotAttached()
        {
            var contentItem = ContentItem.WithPart(new ContentItem.NullContentPart(), "Article");

            mSut.CacheItem(contentItem, "Detail", new ItemLevelCacheItem("html"));

            mOutputCacheStorageProvider.Verify(p => p.GetCacheItem(It.IsAny<string>()), Times.Never());
            mOutputCacheStorageProvider.Verify(p => p.Set(It.IsAny<string>(), It.IsAny<CacheItem>()), Times.Never());
        }

        [Test]
        public void CacheItem_ReturnsWhenDisplayTypeIsNotConfigured()
        {
            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article");
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings["Summary"] = new ItemLevelCacheSettings();

            mSut.CacheItem(contentItem, "Detail", new ItemLevelCacheItem("html"));

            mOutputCacheStorageProvider.Verify(p => p.GetCacheItem(It.IsAny<string>()), Times.Never());
            mOutputCacheStorageProvider.Verify(p => p.Set(It.IsAny<string>(), It.IsAny<CacheItem>()), Times.Never());
        }

        [TestCase(ItemLevelCacheMode.CacheItem, true)]
        [TestCase(ItemLevelCacheMode.DoNothing, false)]
        [TestCase(ItemLevelCacheMode.ExcludeFromPageCache, false)]
        public void CacheItem_ReturnsIfDisplayTypeNotSetToCacheItem(ItemLevelCacheMode mode, bool shouldInvokeStorageProvider)
        {
            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article", 42);
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings["Detail"] = new ItemLevelCacheSettings { Mode = mode };

            mSut.CacheItem(contentItem, "Detail", new ItemLevelCacheItem("html"));

            mOutputCacheStorageProvider.Verify(p => p.GetCacheItem(It.IsAny<string>()), Times.Exactly(shouldInvokeStorageProvider ? 1 : 0));
            mOutputCacheStorageProvider.Verify(p => p.Set(It.IsAny<string>(), It.IsAny<CacheItem>()), Times.Exactly(shouldInvokeStorageProvider ? 1 : 0));
        }

        [TestCase("AlwaysHit", false)]
        [TestCase("AlwaysMiss", true)]
        public void CacheItem_DoesntSetIfCacheKeyAlreadyExists(string displayType, bool shouldSet)
        {
            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article", 42);
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings[displayType] = new ItemLevelCacheSettings { Mode = ItemLevelCacheMode.CacheItem };

            mSut.CacheItem(contentItem, displayType, new ItemLevelCacheItem("html"));

            mOutputCacheStorageProvider.Verify(p => p.GetCacheItem(It.IsAny<string>()), Times.Exactly(1));
            mOutputCacheStorageProvider.Verify(p => p.Set(It.IsAny<string>(), It.IsAny<CacheItem>()), Times.Exactly(shouldSet ? 1 : 0));
        }

        [TestCase(60, 60, "html")]
        [TestCase(0, 60, "£100")]
        [TestCase(60, 0, "%^&*(£@")]
        [TestCase(45, 15, " ")]
        [TestCase(99999, 88888, "<html><body><p class=\"title\">Hello, world!</p></body></html>")]
        public void CacheItem_CachedItemIsAsExpected(int cacheDuration, int graceDuration, string markup)
        {
            var displayType = "AlwaysMiss";

            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article", 42);
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings[displayType] = new ItemLevelCacheSettings
            {
                Mode = ItemLevelCacheMode.CacheItem,
                CacheDurationSeconds = cacheDuration,
                CacheGraceTimeSeconds = graceDuration
            };

            var cacheKey = mSut.GetCacheKey(contentItem, displayType);

            mSut.CacheItem(contentItem, displayType, new ItemLevelCacheItem(markup));

            var cachedItem = mCachedItems[cacheKey];

            Assert.AreEqual(cacheKey, cachedItem.CacheKey);
            Assert.AreEqual(cacheKey, cachedItem.InvariantCacheKey);

            Assert.AreEqual(cacheDuration, cachedItem.Duration);
            Assert.AreEqual(graceDuration, cachedItem.GraceTime);
            Assert.AreEqual(markup, mJsonConvertor.Deserialize<ItemLevelCacheItem>(Encoding.UTF8.GetString(cachedItem.Output)).Markup);
            Assert.AreEqual(mShellSettings.Name, cachedItem.Tenant);
            Assert.AreEqual(mClock.Object.UtcNow, cachedItem.CachedOnUtc);

            Assert.AreEqual(string.Empty, cachedItem.Url);
            
            Assert.Contains(ItemLevelCacheTag.For(contentItem), cachedItem.Tags);
            Assert.Contains(ItemLevelCacheTag.For(contentItem, displayType), cachedItem.Tags);
        }

        [Test]
        public void CacheItem_EveryTagIsInsertedIntoTagCache()
        {
            var displayType = "AlwaysMiss";

            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article", 42);
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings[displayType] = new ItemLevelCacheSettings { Mode = ItemLevelCacheMode.CacheItem };

            var cacheKey = mSut.GetCacheKey(contentItem, displayType);

            mSut.CacheItem(contentItem, displayType, new ItemLevelCacheItem("html"));

            var cachedItem = mCachedItems[cacheKey];

            foreach (var tag in cachedItem.Tags)
            {
                _tagCache.Verify(tc => tc.Tag(tag, cacheKey), Times.Once());
            }
        }

        [Test]
        public void GetCacheKey_ReturnsNullWhenContentItemIsNull()
        {
            Assert.AreEqual(null, mSut.GetCacheKey(null, "Detail"));
        }

        [Test]
        public void GetCacheKey_ReturnsNullWhenDonutCachePartIsNotAttached()
        {
            var contentItem = ContentItem.WithPart(new ContentItem.NullContentPart(), "Article");

            Assert.AreEqual(null, mSut.GetCacheKey(contentItem, "Detail"));
        }

        [Test]
        public void GetCacheKey_ReturnsNullWhenDisplayTypeIsNotConfigured()
        {
            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article");
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings["Summary"] = new ItemLevelCacheSettings();

            Assert.AreEqual(null, mSut.GetCacheKey(contentItem, "Detail"));
        }

        [TestCase(10, "Detail", "ItemLevelCache; 10; Detail; ")]
        [TestCase(20, "Summary", "ItemLevelCache; 20; Summary; ")]
        [TestCase(42, "SearchResult", "ItemLevelCache; 42; SearchResult; ")]
        public void GetCacheKey_CacheKeyPrefixIsAsExpected(int contentItemId, string displayType, string expectedValue)
        {
            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article", contentItemId);
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings[displayType] = new ItemLevelCacheSettings();

            Assert.AreEqual(expectedValue, mSut.GetCacheKey(contentItem, displayType));
        }

        [Test, Combinatorial]
        public void GetCacheKey_OnlyEnabledCompositeProvidersAreAssessed([Values(true, false)] bool aEnabled, [Values(true, false)] bool bEnabled, [Values(true, false)] bool cEnabled, [Values(true, false)] bool dEnabled)
        {
            var mockA = CompositeCacheKeyHelper.CreateMock("a");
            var mockB = CompositeCacheKeyHelper.CreateMock("b");
            var mockC = CompositeCacheKeyHelper.CreateMock("c");
            var mockD = CompositeCacheKeyHelper.CreateMock("d");

            mCompositeCacheKeyProviders.Add(mockA.Object);
            mCompositeCacheKeyProviders.Add(mockB.Object);
            mCompositeCacheKeyProviders.Add(mockC.Object);
            mCompositeCacheKeyProviders.Add(mockD.Object);

            Setup();

            var providerSettings = new Dictionary<string, bool>
            {
                { mockA.Object.TechnicalName, aEnabled },
                { mockB.Object.TechnicalName, bEnabled },
                { mockC.Object.TechnicalName, cEnabled },
                { mockD.Object.TechnicalName, dEnabled },
            };

            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article");
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings["Detail"] = new ItemLevelCacheSettings { CompositeCacheKeyProviders = providerSettings };

            mSut.GetCacheKey(contentItem, "Detail");

            mockA.Verify(p => p.GetCacheKeyValue(), Times.Exactly(aEnabled ? 1 : 0));
            mockB.Verify(p => p.GetCacheKeyValue(), Times.Exactly(bEnabled ? 1 : 0));
            mockC.Verify(p => p.GetCacheKeyValue(), Times.Exactly(cEnabled ? 1 : 0));
            mockD.Verify(p => p.GetCacheKeyValue(), Times.Exactly(dEnabled ? 1 : 0));
        }

        [Test]
        public void GetCacheKey_CompositeProvidersAreDeterminiticallyOrdered()
        {
            // No matter which order the providers are evaluated, the resulting cache key must always be the same
            var combinationStrings = new[]
            {
                "abcd",
                "abdc",
                "acbd",
                "acdb",
                "adbc",
                "adcb",
                "badc",
                "bacd",
                "bcda",
                "bcad",
                "bdca",
                "bdac",
                "cabd",
                "cadb",
                "cbad",
                "cbda",
                "cdab",
                "cdba",
                "dacb",
                "dabc",
                "dbca",
                "dbac",
                "dcba",
                "dcab"
            };

            var providerSettings = new Dictionary<string, bool>
            {
                { "a", true },
                { "b", true },
                { "c", true },
                { "d", true },
            };

            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article");
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings["Detail"] = new ItemLevelCacheSettings { CompositeCacheKeyProviders = providerSettings };

            var cacheKeys = new List<string>();

            foreach (var combinationString in combinationStrings)
            {
                mCompositeCacheKeyProviders.Clear();

                foreach (var c in combinationString.ToCharArray())
                {
                    mCompositeCacheKeyProviders.Add(CompositeCacheKeyHelper.CreateMock(c.ToString()).Object);
                }

                Setup();

                cacheKeys.Add(mSut.GetCacheKey(contentItem, "Detail"));
            }

            Assert.AreEqual(combinationStrings.Length, cacheKeys.Count, "The number of generated cache keys does not match the number of times get cache key should have been called. This is more than likely an issue with the test, as opposed to an issue with the service being tested.");
            Assert.AreEqual(1, cacheKeys.Distinct().Count());
        }

        [Test]
        public void MarkupShouldBeTokenized_ReturnsFalseWhenContentItemIsNull()
        {
            Assert.AreEqual(false, mSut.MarkupShouldBeTokenized(null, "Detail"));
        }

        [Test]
        public void MarkupShouldBeTokenized_ReturnsFalseWhenDonutCachePartIsNotAttached()
        {
            var contentItem = ContentItem.WithPart(new ContentItem.NullContentPart(), "Article");

            Assert.AreEqual(false, mSut.MarkupShouldBeTokenized(contentItem, "Detail"));
        }

        [Test]
        public void MarkupShouldBeTokenized_ReturnsFalseWhenDisplayTypeIsNotConfigured()
        {
            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article");
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings["Summary"] = new ItemLevelCacheSettings { Mode = ItemLevelCacheMode.CacheItem };

            Assert.AreEqual(false, mSut.MarkupShouldBeTokenized(contentItem, "Detail"));
        }

        [TestCase(ItemLevelCacheMode.CacheItem, true)]
        [TestCase(ItemLevelCacheMode.ExcludeFromPageCache, true)]
        [TestCase(ItemLevelCacheMode.DoNothing, false)]
        public void MarkupShouldBeTokenized_ReturnsTrueDisplayTypeIsConfiguredAndRequired(ItemLevelCacheMode mode, bool expectedValue)
        {
            var contentItem = ContentItem.WithPart(new ItemLevelCachePart(), "Article");
            contentItem.As<ItemLevelCachePart>().ItemLevelCacheSettings["Detail"] = new ItemLevelCacheSettings { Mode = mode };

            Assert.AreEqual(expectedValue, mSut.MarkupShouldBeTokenized(contentItem, "Detail"));
        }

        [TestCase("hit", "html")]
        [TestCase("miss", null)]
        public void GetCacheItem_ReturnsNullOnMissAndMarkupOnHit(string cacheKey, string expectedValue)
        {
            Assert.AreEqual(expectedValue, mSut.GetCacheItem(cacheKey)?.ItemLevelCacheItem?.Markup);
        }
    }
}
