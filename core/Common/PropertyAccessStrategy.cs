using System;
using System.Collections.Generic;
using System.Reflection;

namespace NAxonFramework.Common
{
    public abstract class PropertyAccessStrategy : IComparable<PropertyAccessStrategy>
    {
        public int CompareTo(PropertyAccessStrategy o)
        {
            if (o == this) 
            {
                return 0;
            }
            int diff = o.Priority - Priority;
            if (diff == 0) 
            {
                // we don't want equality...
                //todo: does this make sense??
                return GetType().Name.CompareTo(o.GetType().Name);
            }
            return diff;
        }

        private static ICollection<PropertyAccessStrategy> _strategies = new SortedSet<PropertyAccessStrategy>();
        private static object _lock => ((System.Collections.ICollection) _strategies).SyncRoot;

        public static void Register(PropertyAccessStrategy strategy)
        {
            lock(_lock)
            {
                _strategies.Add(strategy);
            };  
        }

        public static void Unregister(PropertyAccessStrategy strategy)
        {
            lock (_lock)
            {
                _strategies.Remove(strategy);
            }
        }

        public static PropertyInfo GetProperty(Type targetClass, string propertyName)
        {
            PropertyInfo property = null;
            
            lock (_lock)
            {
                var strategies = _strategies.GetEnumerator();
                while (property == null && strategies.MoveNext())
                {
                    property = strategies.Current.PropertyFor(targetClass, propertyName);
                }
            }

            return property;
        }
        
        protected abstract int Priority { get; }
        protected abstract PropertyInfo PropertyFor(Type targetClass, string property);
    }
}