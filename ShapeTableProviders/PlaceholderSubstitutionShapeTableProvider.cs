using Orchard.DisplayManagement.Descriptors;

namespace IDeliverable.Donuts.ShapeTableProviders
{
    public class PlaceholderSubstitutionShapeTableProvider : IShapeTableProvider
    {
        public PlaceholderSubstitutionShapeTableProvider(IPlaceholderSubstitutionShapeTableService placeholderSubstitutionShapeTableService)
        {
            mPlaceholderSubstitutionShapeTableService = placeholderSubstitutionShapeTableService;
        }

        private readonly IPlaceholderSubstitutionShapeTableService mPlaceholderSubstitutionShapeTableService;

        public void Discover(ShapeTableBuilder builder)
        {
            // WARNING: License check will always fail if attempted from inside an IShapeTableProvider.

            foreach (var shapeType in new[] { "Content", "Widget" })
            {
                builder
                    .Describe(shapeType)
                    .OnDisplaying(context => mPlaceholderSubstitutionShapeTableService.OnDisplaying(context));
            }
        }
    }
}