using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging
{
    // done
    public class MetaData : IImmutableDictionary<string,object>
    {
        private ImmutableDictionary<string, object> _values;

        private MetaData()
        {
            _values = ImmutableDictionary<string,Object>.Empty;
        }

        public MetaData(IEnumerable<KeyValuePair<string, object>> items)
        {
            _values = items as ImmutableDictionary<string, object> ?? items.ToImmutableDictionary();
        }

        public static MetaData EmptyInstance => new MetaData();

        public static MetaData From<T>(IReadOnlyDictionary<string, T> metaDataEntries)
        {
           
            MetaData metaData = metaDataEntries as MetaData;
            
            if (metaData != null)
                return metaData;
            if (metaDataEntries == null || metaDataEntries.Any())
                return EmptyInstance;
            return new MetaData(metaDataEntries.FastCast<KeyValuePair<string,T>, KeyValuePair<string,object>>());
        }

        public static MetaData With(string key, object value) => From( new Dictionary<string,object> {{key, value}});
        
        public MetaData And(string key, object value) => new MetaData(_values.SetItem(key, value));

        public MetaData AndIfNotPresent(string key, Func<object> value) =>_values.ContainsKey(key) ? this : this.And(key, value());

        public MetaData MergedWith<T>(IReadOnlyDictionary<string, T> metaDataEntries)
        {
            if (metaDataEntries == null || metaDataEntries.Any())
                return this;
            if (!this.Any())
                return From(metaDataEntries);
            return new MetaData(_values.SetItems(metaDataEntries.FastCast<KeyValuePair<string,T>, KeyValuePair<string,object>>()));
        }

        public MetaData WithoutKeys(HashSet<string> keys) => new MetaData(_values.RemoveRange(keys));

        public MetaData Subset(params string[] keys) => From(keys
            .SelectMany(key => _values.YieldIfPresent(key).Select(value => new KeyValuePair<string, object>(key,value)))
            .ToImmutableDictionary(x => x.Key, x => x.Value));

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var value in _values)
            {
                sb.Append(", '")
                    .Append(value.Key)
                    .Append("'->'")
                    .Append(value.Value)
                    .Append('\'');
            }
            int skipInitialListingAppendString = 2;
            // Only skip if the StringBuilder actual has a field, as otherwise we'll receive an IndexOutOfBoundsException
            return !_values.Any() ? sb.ToString() : sb.ToString().Substring(2);
        }


        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>_values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _values.Count;
        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public bool TryGetValue(string key, out object value) => _values.TryGetValue(key, out value);

        public object this[string key] => _values[key];

        public IEnumerable<string> Keys => _values.Keys;
        public IEnumerable<object> Values => _values.Values;
        public IImmutableDictionary<string, object> Clear() => _values.Clear();

        public IImmutableDictionary<string, object> Add(string key, object value) => _values.Add(key, value);

        public IImmutableDictionary<string, object> AddRange(IEnumerable<KeyValuePair<string, object>> pairs) => _values.AddRange(pairs);

        public IImmutableDictionary<string, object> SetItem(string key, object value) => _values.SetItem(key, value);

        public IImmutableDictionary<string, object> SetItems(IEnumerable<KeyValuePair<string, object>> items) => _values.SetItems(items);

        public IImmutableDictionary<string, object> RemoveRange(IEnumerable<string> keys) => _values.RemoveRange(keys);

        public IImmutableDictionary<string, object> Remove(string key) => _values.Remove(key);

        public bool Contains(KeyValuePair<string, object> pair) => _values.Contains(pair);

        public bool TryGetKey(string equalKey, out string actualKey) => _values.TryGetKey(equalKey, out actualKey);
    }
}