using Orchard;
using Orchard.DisplayManagement.Implementation;

namespace IDeliverable.Donuts.ShapeTableProviders
{
    public interface IPlaceholderSubstitutionShapeTableService : IDependency
    {
        void OnDisplaying(ShapeDisplayingContext context);
    }
}