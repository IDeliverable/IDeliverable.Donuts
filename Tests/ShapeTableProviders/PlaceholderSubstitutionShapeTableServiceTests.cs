//using System.Collections.Generic;
//using System.Dynamic;
//using IDeliverable.DonutCache.Services;
//using IDeliverable.DonutCache.ShapeTableProviders;
//using IDeliverable.DonutCache.Tests.Stubs;
//using Moq;
//using NUnit.Framework;
//using Orchard.ContentManagement;
//using Orchard.DisplayManagement.Implementation;
//using Orchard.DisplayManagement.Shapes;
//using Orchard.Mvc;
//using Orchard.UI.Admin;

//namespace IDeliverable.DonutCache.Tests.ShapeTableProviders
//{
//    [TestFixture]
//    public class PlaceholderSubstitutionShapeTableServiceTests
//    {
//        private PlaceholderSubstitutionShapeTableService _sut;

//        private Mock<IDonutCacheService> _donutCacheService;
//        private Mock<IMarkupSessionStore> _markupSessionStore;
//        private Mock<IHttpContextAccessor> _httpContextAccessor;

//        [SetUp]
//        public void SetUp()
//        {
//            _donutCacheService = new Mock<IDonutCacheService>();
//            _markupSessionStore = new Mock<IMarkupSessionStore>();
//            _httpContextAccessor = new Mock<IHttpContextAccessor>();

//            _donutCacheService.Setup(s=>s.GetCacheKey(It.IsAny<IContent>(), It.IsAny<string>())).Returns<IContent, string>((content, displayType) => displayType);

//            _donutCacheService.Setup(s => s.GetCacheItem("Hit1")).Returns("abc");
//            _donutCacheService.Setup(s => s.GetCacheItem("Hit2")).Returns("xyz");
//            _donutCacheService.Setup(s => s.GetCacheItem("Miss")).Returns(null as string);

//            _httpContextAccessor.Setup(a => a.Current()).Returns(new DonutCacheStubHttpContext());

//            _sut = new PlaceholderSubstitutionShapeTableService(_donutCacheService.Object, _markupSessionStore.Object, _httpContextAccessor.Object);
//        }

//        [Test]
//        public void OnDisplaying_ExitsIfAdminIsApplied()
//        {
//            AdminFilter.Apply(_httpContextAccessor.Object.Current().Request.RequestContext);

//            var context = new ShapeDisplayingContext();
//            _sut.OnDisplaying(context);

//            _donutCacheService.Verify(s => s.GetCacheKey(It.IsAny<IContent>(), It.IsAny<string>()), Times.Never());
//            _donutCacheService.Verify(s => s.GetCacheItem(It.IsAny<string>()), Times.Never());
//        }

//        [TestCase(null, false)]
//        [TestCase("CacheKey1", true)]
//        [TestCase("CacheKey2", true)]
//        public void OnDisplaying_GetsCacheItemIfCacheKeyIsAvailable(string displayType, bool cacheKeyAvailable)
//        {
//            dynamic shape = new ExpandoObject();

//            shape.ContentItem = null;

//            var context = new ShapeDisplayingContext
//            {
//                Shape = shape,
//                ShapeMetadata = new ShapeMetadata
//                {
//                    DisplayType = displayType
//                }
//            };

//            _sut.OnDisplaying(context);

//            _donutCacheService.Verify(s => s.GetCacheKey(It.IsAny<IContent>(), It.IsAny<string>()), Times.Once());
//            _donutCacheService.Verify(s => s.GetCacheItem(displayType), cacheKeyAvailable ? Times.Once() : Times.Never());
//        }
        
//        [TestCase("Hit1", true, "abc")]
//        [TestCase("Hit2", true, "xyz")]
//        [TestCase("Miss", false, null)]
//        public void OnDisplaying_SetsWrappersAndChildContentIfCacheHit(string displayType, bool cacheHit, string cachedMarkup)
//        {
//            // Just to be clear what's going on
//            var cacheKey = displayType;
//            var wrappers = new List<string>
//            {
//                "Wrapper A", "Wrapper B"
//            };

//            dynamic shape = new ExpandoObject();

//            shape.ContentItem = null;

//            var context = new ShapeDisplayingContext
//            {
//                Shape = shape,
//                ShapeMetadata = new ShapeMetadata
//                {
//                    DisplayType = displayType,
//                    Wrappers = wrappers
//                }
//            };

//            _sut.OnDisplaying(context);
            

//            if (cacheHit)
//            {
//                // Verify that the wrappers are removed
//                Assert.AreEqual(0, context.ShapeMetadata.Wrappers.Count);
//                // And that the ChildContent is set the the cached markup
//                Assert.AreEqual(cachedMarkup, context.ChildContent.ToHtmlString());
//            }
//            else
//            {
//                // Verify that the wrappers are left intact
//                Assert.AreSame(wrappers, context.ShapeMetadata.Wrappers);
//                Assert.AreEqual(cachedMarkup, null);
//            }
//        }
//    }
//}
