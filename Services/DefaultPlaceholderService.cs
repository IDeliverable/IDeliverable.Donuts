using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IDeliverable.Donuts.Providers;

namespace IDeliverable.Donuts.Services
{
    public class DefaultPlaceholderService : IPlaceholderService
    {
        public DefaultPlaceholderService(IEnumerable<IPlaceholderProvider> placeholderProviders)
        {
            mPlaceholderProviders = placeholderProviders;
        }

        private readonly IEnumerable<IPlaceholderProvider> mPlaceholderProviders;

        public string ResolvePlaceholders(string targetText)
        {
            if (targetText == null)
                return null;

            return ResolveRecursively(targetText, new Dictionary<string, string>());
        }

        private string ResolveRecursively(string targetText, IDictionary<string, string> resolvedPlaceholders)
        {
            var matches = Regex.Matches(targetText, @"%%{(.*?)}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

            // Get distinct to prevent the same placeholder from being resolved multiple times.
            var distinctPlaceholders =
                matches
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .Distinct()
                    .ToList();

            // If there aren't any unresolved placeholders in the target text, then we are done.
            if (!distinctPlaceholders.Any())
                return targetText;

            // If all the placeholders in the target text have already been attempted to be resolved, but are null, then we are done.
            if (distinctPlaceholders.All(p => resolvedPlaceholders.ContainsKey(p) && resolvedPlaceholders[p] == null))
                return targetText;

            foreach (var placeholder in distinctPlaceholders)
            {
                // If the placeholder already exists in the dictionary, no need to try and resolve it again.
                // This would happen if this method is being called recursively and the placeholder has been resolved in a previous iteration.
                if (resolvedPlaceholders.ContainsKey(placeholder))
                    continue;

                var resolvedValue = mPlaceholderProviders.Select(p => p.ResolvePlaceholder(placeholder)).FirstOrDefault(v => v != null);
                
                resolvedPlaceholders.Add(placeholder, resolvedValue);
            }

            var stringBuilder = new StringBuilder(targetText);

            // Replace cache tokens in the original text with resolved markup.
            foreach (var resolvedCacheToken in resolvedPlaceholders.Where(p => p.Value != null))
                stringBuilder.Replace($"%%{{{resolvedCacheToken.Key}}}", resolvedCacheToken.Value);

            return ResolveRecursively(stringBuilder.ToString(), resolvedPlaceholders);
        }
    }
}