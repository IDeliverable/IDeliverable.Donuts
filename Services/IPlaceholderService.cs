using Orchard;

namespace IDeliverable.Donuts.Services
{
    public interface IPlaceholderService : IDependency
    {
        string ResolvePlaceholders(string targetText);
    }
}