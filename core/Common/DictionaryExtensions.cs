using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NAxonFramework.Common
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class DictionaryExtensions
    {
        public static IEnumerable<V> YieldIfPresent<K, V>(this IReadOnlyDictionary<K,V> dictionary, K key)
        {
            if (dictionary.TryGetValue(key, out var value))
                yield return value;
        }

        
        public static V GetValueOrDefault<K, V>(this IEnumerable<KeyValuePair<K, V>> source, K key, V @default = default(V))
        {
            switch (source)
            {
                case IDictionary<K,V> dictionary:
                    return dictionary.TryGetValue(key, out var value1) ? value1 : @default;
                case IReadOnlyDictionary<K,V> dictionary:
                    return dictionary.TryGetValue(key, out var value2) ? value2 : @default;
                default:
                    throw new ArgumentException("Must be IDictionary or IReadOnlyDictionry", nameof(source));
            }
            
        }

        public static V PutIfAbsent<K, V>(this IDictionary<K, V> dictionary, K key, V value)
        {
            if (!dictionary.TryGetValue(key, out var existing))
            {
                dictionary[key] = value;
                return value;
            }
            else
            {
                return existing;
            }

        }
//        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, V @default = default(V))
//        {
//            return dictionary.TryGetValue(key, out var value) ? value : @default;
//        }

        public static V ComputeIfAbsent<K, V>(this IDictionary<K, V> dictionary, K key, Func<K, V> mappingFunction)
        {
            
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = mappingFunction(key);
                if (value != null)
                    dictionary[key] = value;
            }

            return value;
        }
    }
}