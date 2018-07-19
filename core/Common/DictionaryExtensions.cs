using System.Collections.Generic;
using System.Collections.Immutable;

namespace NAxonFramework.Common
{
    public static class DictionaryExtensions
    {
        public static IEnumerable<V> YieldIfPresent<K, V>(this IReadOnlyDictionary<K,V> dictionary, K key)
        {
            if (dictionary.TryGetValue(key, out var value))
                yield return value;
        }

        public static V GetValueOrDefault<K, V>(this IReadOnlyDictionary<K, V> dictionary, K key, V @default = default(V))
        {
            return dictionary.TryGetValue(key, out var value) ? value : @default;
        }

    }
}