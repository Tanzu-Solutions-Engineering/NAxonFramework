using System;

namespace NAxonFramework.Serialization
{
    public interface ISerializer
    {
        ISerializedObject Serialize(object obj, Type expectedRepresentation);
        ISerializedObject<T> Serialize<T>(object obj);
        bool CanSerializeTo(Type type);
        bool CanSerializeTo<T>();
        object Deserialize(ISerializedObject obj);
        T Deserialize<T>(ISerializedObject<T> obj);
        Type ClassForType(ISerializedType obj);
        ISerializedType TypeForClass(Type type);
        IConverter Converter { get; }
    }
}