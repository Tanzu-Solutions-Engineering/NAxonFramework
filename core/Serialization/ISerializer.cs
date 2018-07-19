using System;

namespace NAxonFramework.Serialization
{
    public interface ISerializer
    {
        ISerializedObject<T> Serialize<T>(object obj);
        bool CanSerializeTo<T>();
        T Deserialize<T>(ISerializedObject obj);
        Type ClassForType(ISerializedType obj);
        ISerializedType TypeForClass(Type type);
        IConverter Converter { get; }
    }
}