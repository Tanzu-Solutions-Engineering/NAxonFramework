using System;
using Microsoft.DotNet.PlatformAbstractions;

namespace NAxonFramework.Serialization
{
    public class SimpleSerializedType : ISerializedType
    {
        public string Name { get; }
        public string Revision { get; }
        private static SimpleSerializedType EMPTY_TYPE = new SimpleSerializedType("empty", null);
        public SimpleSerializedType(string type, string revision)
        {
            Name = type;
            Revision = revision;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }
            if (o == null || GetType() != o.GetType()) {
                return false;
            }
            var that = (ISerializedType) o;
            return Object.Equals(Name, that.Name) && Object.Equals(Revision, that.Revision);
        }

        public override int GetHashCode()
        {
            var hash = HashCodeCombiner.Start();
            hash.Add(Name);
            hash.Add(Revision);
            return hash.CombinedHash;
        }

        public override string ToString() => $"SimpleSerializedType{Name} (revision {Revision})";
    }
}