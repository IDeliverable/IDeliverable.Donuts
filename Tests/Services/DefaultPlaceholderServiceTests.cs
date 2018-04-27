using System.Collections.Generic;
using IDeliverable.Donuts.Providers;
using IDeliverable.Donuts.Services;
using Moq;
using NUnit.Framework;

namespace IDeliverable.Donuts.Tests.Services
{
    [TestFixture]
    public class DefaultPlaceholderServiceTests
    {
        private DefaultPlaceholderService _sut;
        private Mock<IPlaceholderProvider> _mockPlaceholderProvider;

        private Dictionary<string, string> _placeholderResults;
        
        [SetUp]
        public void Setup()
        {
            _placeholderResults = new Dictionary<string, string>
            {
                { "username", "chris" },
                { "day of week", "monday" },
                { "year", "1985" }
            };

            _mockPlaceholderProvider = new Mock<IPlaceholderProvider>();
            _mockPlaceholderProvider.Setup(p => p.ResolvePlaceholder(It.IsAny<string>())).Returns<string>(placeholder =>
            {
                if (_placeholderResults.ContainsKey(placeholder))
                {
                    return _placeholderResults[placeholder];
                }

                return null;
            });

            _sut = new DefaultPlaceholderService(new[] { _mockPlaceholderProvider.Object });
        }

        [Test]
        public void ResolvePlaceholders_ShouldReturnNullIfTargetTextIsNull()
        {
            Assert.AreEqual(null, _sut.ResolvePlaceholders(null));
        }

        public IEnumerable<KeyValuePair<string, string[]>> ResolvePlaceholders_ShouldCorrectlyIdentifyAllPlaceholders_Source = new[]
        {
            new KeyValuePair<string, string[]>("%%{username}", new [] {"username"}),
            new KeyValuePair<string, string[]>("%%{year}", new [] {"year"}),
            new KeyValuePair<string, string[]>("text at start %%{year}", new [] {"year"}),
            new KeyValuePair<string, string[]>("%%{year} text at end", new [] {"year"}),
            new KeyValuePair<string, string[]>("text at start %%{year} and end", new [] {"year"}),
            new KeyValuePair<string, string[]>("%%{more} %%{than} %%{one}", new [] {"more", "than", "one"}),
            new KeyValuePair<string, string[]>("%%{$%^}", new [] {"$%^"}),
            new KeyValuePair<string, string[]>("%%{one with spaces}", new [] {"one with spaces"}),
        };

        [TestCaseSource(nameof(ResolvePlaceholders_ShouldCorrectlyIdentifyAllPlaceholders_Source))]
        public void ResolvePlaceholders_ShouldCorrectlyIdentifyAllPlaceholders(KeyValuePair<string, string[]> targetText)
        {
            _sut.ResolvePlaceholders(targetText.Key);

            foreach (var expectedValue in targetText.Value)
            {
                _mockPlaceholderProvider.Verify(p => p.ResolvePlaceholder(expectedValue), Times.AtLeastOnce());
            }
        }

        public IEnumerable<KeyValuePair<string, string[]>> ResolvePlaceholders_ShouldIdentifyOnlyDistinctPlaceholders_Source = new[]
        {
            new KeyValuePair<string, string[]>("%%{username}", new [] {"username"}),
            new KeyValuePair<string, string[]>("%%{username}%%{username}", new [] {"username"}),
            new KeyValuePair<string, string[]>("%%{username} %%{username}", new [] {"username"}),
            new KeyValuePair<string, string[]>("%%{username} %%{username} %%{year}", new [] {"username", "year"}),
        };

        [TestCaseSource(nameof(ResolvePlaceholders_ShouldIdentifyOnlyDistinctPlaceholders_Source))]
        public void ResolvePlaceholders_ShouldIdentifyOnlyDistinctPlaceholders(KeyValuePair<string, string[]> targetText)
        {
            _sut.ResolvePlaceholders(targetText.Key);

            foreach (var expectedValue in targetText.Value)
            {
                _mockPlaceholderProvider.Verify(p => p.ResolvePlaceholder(expectedValue), Times.Once());
            }
        }

        [TestCase("%%{username}", "chris")]
        [TestCase("hello, %%{username}", "hello, chris")]
        [TestCase("hello, %%{username}. you were born in %%{year}", "hello, chris. you were born in 1985")]
        [TestCase("hello, %%{username}. you were born in %%{year}. %%{this placeholder will not resolve}", "hello, chris. you were born in 1985. %%{this placeholder will not resolve}")]
        public void ResolvePlaceholders_ReplacesTextAsExpected(string targetText, string expectedValue)
        {
            Assert.AreEqual(expectedValue, _sut.ResolvePlaceholders(targetText));
        }

        [Test]
        public void ResolvePlaceholders_FirstProviderWinsInConflict()
        {
            var mockPlaceholderProvider1 = new Mock<IPlaceholderProvider>();
            var mockPlaceholderProvider2 = new Mock<IPlaceholderProvider>();
            var mockPlaceholderProvider3 = new Mock<IPlaceholderProvider>();

            mockPlaceholderProvider1.Setup(p => p.ResolvePlaceholder(It.IsAny<string>())).Returns("1");
            mockPlaceholderProvider2.Setup(p => p.ResolvePlaceholder(It.IsAny<string>())).Returns("2");
            mockPlaceholderProvider3.Setup(p => p.ResolvePlaceholder(It.IsAny<string>())).Returns("3");

            _sut = new DefaultPlaceholderService(new[]
            {
                mockPlaceholderProvider1.Object,
                mockPlaceholderProvider2.Object,
                mockPlaceholderProvider3.Object,
            });
            Assert.AreEqual("1", _sut.ResolvePlaceholders("%%{test}"));

            _sut = new DefaultPlaceholderService(new[]
            {
                mockPlaceholderProvider2.Object,
                mockPlaceholderProvider3.Object,
                mockPlaceholderProvider1.Object,
            });
            Assert.AreEqual("2", _sut.ResolvePlaceholders("%%{test}"));

            _sut = new DefaultPlaceholderService(new[]
            {
                mockPlaceholderProvider3.Object,
                mockPlaceholderProvider1.Object,
                mockPlaceholderProvider2.Object,
            });
            Assert.AreEqual("3", _sut.ResolvePlaceholders("%%{test}"));
        }
    }
}
