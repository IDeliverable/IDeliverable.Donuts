using IDeliverable.Donuts.Providers;
using Moq;

namespace IDeliverable.Donuts.Tests
{
    public class CompositeCacheKeyHelper
    {
        public static Mock<ICompositeCacheKeyProvider> CreateMock(string value)
        {
            var mock = new Mock<ICompositeCacheKeyProvider>();

            mock.Setup(p => p.TechnicalName).Returns(value);
            mock.Setup(p => p.GetCacheKeyValue()).Returns(value);

            return mock;
        }
    }
}
