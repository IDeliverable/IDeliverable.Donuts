using System.Collections.Generic;
using System.Linq;

namespace IDeliverable.Donuts.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> passiveDictionary)
        {
            return source.Concat(passiveDictionary.Where(kvp => !source.ContainsKey(kvp.Key))).ToDictionary(kvp=>kvp.Key, kvp=>kvp.Value);
        }
    }
}