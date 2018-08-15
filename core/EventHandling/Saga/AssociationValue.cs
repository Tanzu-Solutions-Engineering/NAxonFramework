using System.Collections.Generic;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling.Saga
{
    public class AssociationValue
    {
        private sealed class PropertyValuePropertyKeyEqualityComparer : IEqualityComparer<AssociationValue>
        {
            public bool Equals(AssociationValue x, AssociationValue y)
            {
                return string.Equals(x.PropertyValue, y.PropertyValue) && string.Equals(x.PropertyKey, y.PropertyKey);
            }

            public int GetHashCode(AssociationValue obj)
            {
                unchecked
                {
                    return ((obj.PropertyValue != null ? obj.PropertyValue.GetHashCode() : 0) * 397) ^ (obj.PropertyKey != null ? obj.PropertyKey.GetHashCode() : 0);
                }
            }
        }

        public static IEqualityComparer<AssociationValue> PropertyValuePropertyKeyComparer { get; } = new PropertyValuePropertyKeyEqualityComparer();

        public AssociationValue(string key, string value)
        {
            Assert.NotNull(key, () => "Cannot associate a Saga with a null key");
            Assert.NotNull(value, () => "Cannot associate a Saga with a null value");
            this.PropertyKey = key;
            this.PropertyValue = value;
        }

        public string PropertyValue { get; set; }

        public string PropertyKey { get; set; }
    }
}