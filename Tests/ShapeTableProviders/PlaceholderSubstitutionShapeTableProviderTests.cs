using System;
using System.Linq;
using IDeliverable.Donuts.ShapeTableProviders;
using Moq;
using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions.Models;

namespace IDeliverable.Donuts.Tests.ShapeTableProviders
{
    [TestFixture]
    public class PlaceholderSubstitutionShapeTableProviderTests
    {
        private PlaceholderSubstitutionShapeTableProvider _sut;
        private Mock<IPlaceholderSubstitutionShapeTableService> _placeholderSubstitutionShapeTableService = new Mock<IPlaceholderSubstitutionShapeTableService>();

        [SetUp]
        public void SetUp()
        {
            _sut = new PlaceholderSubstitutionShapeTableProvider(_placeholderSubstitutionShapeTableService.Object);
        }

        [Test]
        public void Discover_BindsToTheCorrectShapeTypes()
        {
            var builder = new ShapeTableBuilder(new Feature());
            _sut.Discover(builder);

            var alterations = builder.BuildAlterations();

            Assert.AreEqual(2, alterations.Count());
            Assert.Contains("Content", alterations.Select(a => a.ShapeType).ToArray());
            Assert.Contains("Widget", alterations.Select(a => a.ShapeType).ToArray());
        }
    }
}