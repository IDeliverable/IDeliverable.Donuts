using Orchard;

namespace IDeliverable.Donuts.Providers
{
    public interface IPlaceholderProvider : IDependency
    {
        string ResolvePlaceholder(string placeholderText);
    }
}