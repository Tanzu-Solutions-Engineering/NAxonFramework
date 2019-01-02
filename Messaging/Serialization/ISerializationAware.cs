using System;

namespace NAxonFramework.Serialization
{
    public interface ISerializationAware
    {
        ISerializedObject<T> SerializePayload<T>(ISerializer serializer);
        ISerializedObject<T> SerializeMetaData<T>(ISerializer serializer);
    }

}