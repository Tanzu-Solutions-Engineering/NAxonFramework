using System;

namespace NAxonFramework.Serialization
{
    public interface ISerializedObject
    {
        Type ContentType { get; }
        ISerializedType Type { get; }
        object Data { get; }
    }
    public interface ISerializedObject<out T> : ISerializedObject
    {
        new T Data { get; }
    }
}