using System;
using NAxonFramework.Messaging;

namespace NAxonFramework.Serialization
{
    public class MessageSerializer : ISerializer
    {
        private readonly ISerializer _serializer;

        public MessageSerializer(ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }
        
        public static ISerializedObject<T> SerializePayload<T>(IMessage<T> message, ISerializer serializer)
        {
            var expectedRepresentation = typeof(T);
            if (message is ISerializationAware) 
            {
                return ((ISerializationAware) message).SerializePayload<T>(serializer);
            }
            var serializedObject = serializer.Serialize<T>(message.Payload);
            if (message.Payload == null) {
                // make sure the payload type is maintained
                return new SimpleSerializedObject<T>(serializedObject.Data, serializer.TypeForClass(message.PayloadType));
            }
            return serializedObject;
        }
        
        public static ISerializedObject<T> SerializeMetaData<T>(IMessage<T> message, ISerializer serializer) 
        {
            if (message is ISerializationAware) {
                return ((ISerializationAware) message).SerializeMetaData<T>(serializer);
            }
            return serializer.Serialize<T>(message.MetaData);
        }
        public ISerializedObject<T> SerializePayload<T>(IMessage<T> message) 
        {
            return SerializePayload<T>(message, _serializer);
        }
        public ISerializedObject<T> SerializeMetaData<T>(IMessage<T> message) 
        {
            return SerializeMetaData<T>(message, _serializer);
        }
        public ISerializedObject<T> Serialize<T>(Object @object) 
        {
            return _serializer.Serialize<T>(@object);
        }

        public bool CanSerializeTo<T>()
        {
            return _serializer.CanSerializeTo<T>();
        }

        public T Deserialize<T>(ISerializedObject obj)
        {
            return _serializer.Deserialize<T>(obj);
        }

        public Type ClassForType(ISerializedType obj)
        {
            return _serializer.ClassForType(obj);
        }

        public ISerializedType TypeForClass(Type type)
        {
            return _serializer.TypeForClass(type);
        }

        public IConverter Converter => _serializer.Converter;
    }
}