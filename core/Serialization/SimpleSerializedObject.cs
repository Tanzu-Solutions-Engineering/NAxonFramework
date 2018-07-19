using System;
using Microsoft.DotNet.PlatformAbstractions;

namespace NAxonFramework.Serialization
{
    public class SimpleSerializedObject<T> : ISerializedObject<T>
    {
        public SimpleSerializedObject(T data, ISerializedType serializedType)
        {
            Data = data;
            ContentType = typeof(T);
            Type = serializedType;
            
        }

        public SimpleSerializedObject(T data, string type, string revision) : this(data, new SimpleSerializedType(type,revision))
        {
            
        }

        public T Data { get; }

        public Type ContentType { get; }

        public ISerializedType Type { get; }

        object ISerializedObject.Data => Data;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }
            if (o == null || GetType() != o.GetType()) {
                return false;
            }
            var that = (ISerializedObject) o;
            
            return Object.Equals(Data, that.Data) && Object.Equals(Type, that.Type) && Object.Equals(ContentType, that.ContentType);
        }

        public override int GetHashCode()
        {
            var hashCode = HashCodeCombiner.Start();
            hashCode.Add(Data);
            hashCode.Add(Type);
            hashCode.Add(ContentType);
            return hashCode.CombinedHash;
            
        }

        public override string ToString() => $"SimpleSerializedObject {Type}";
    }
}