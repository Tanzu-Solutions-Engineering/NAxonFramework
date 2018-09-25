using System;
using System.Collections.Generic;

namespace NAxonFramework.Common.Property
{
    public abstract class PropertyAccessStrategy : IComparable<PropertyAccessStrategy>
    {
        private static HashSet<PropertyAccessStrategy> Strategies { get; set; } = new HashSet<PropertyAccessStrategy>();

        public static void Register(PropertyAccessStrategy strategy)
        {
            Strategies.Add(strategy);
        }
        public static void Unregister(PropertyAccessStrategy strategy)
        {
            Strategies.Remove(strategy);
        }
        public static IProperty<T> GetProperty<T>(Type targetClass, string propertyName) 
        {
            IProperty<T> property = null;
            var strategies = Strategies.GetEnumerator();
            while (property == null && strategies.MoveNext()) 
            {
                property = strategies.Current.PropertyFor<T>(targetClass, propertyName);
            }

            return property;
        }

        public int CompareTo(PropertyAccessStrategy o)
        {
            if (o == this) {
                return 0;
            }
            var diff = o.Priority - Priority;
            if (diff == 0) 
            {
                // we don't want equality...
                return this.GetType().Name.CompareTo(o.GetType().Name);
            }
            return diff;
        }
        protected abstract int Priority { get; }
        protected abstract IProperty<T> PropertyFor<T>(Type targetClass, string property);


    }
}