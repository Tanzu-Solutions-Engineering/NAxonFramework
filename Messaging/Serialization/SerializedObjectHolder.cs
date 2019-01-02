using System.Collections.Generic;
using NAxonFramework.Messaging;

namespace NAxonFramework.Serialization
{
    public class SerializedObjectHolder : ISerializationAware
    {
        private readonly IMessage _message;
        private readonly object _payloadGuard = new object();
        // guardreadonly "payloadGuard"
        private readonly Dictionary<ISerializer, ISerializedObject> _serializedPayload = new Dictionary<ISerializer, ISerializedObject>();

        private readonly object _metaDataGuard = new object();
        // guardreadonly "metaDataGuard"
        private readonly Dictionary<ISerializer, ISerializedObject> _serializedMetaData = new Dictionary<ISerializer, ISerializedObject>();
        
        public SerializedObjectHolder(IMessage message) 
        {
            this._message = message;
        }


        public ISerializedObject<T> SerializePayload<T>(ISerializer serializer)
        {
            var expectedRepresentation = typeof(T);
            lock (_payloadGuard) 
            {
                if(!_serializedPayload.TryGetValue(serializer, out var existingForm))
                {
                    var serialized = MessageSerializer.SerializePayload((IMessage<T>)_message, serializer);
                    _serializedPayload[serializer] = serialized;
                    return serialized;
                } else {
                    return serializer.Converter.Convert<T>(existingForm);
                }
            }
        }

        public ISerializedObject<T> SerializeMetaData<T>(ISerializer serializer)
        {
            var expectedRepresentation = typeof(T);
            lock (_metaDataGuard) 
            {
                if(!_serializedMetaData.TryGetValue(serializer, out var existingForm))
                {
                    var serialized = MessageSerializer.SerializeMetaData<T>((IMessage<T>)_message, serializer);
                    _serializedMetaData[serializer] = serialized;
                    return serialized;
                } else {
                    return serializer.Converter.Convert<T>(existingForm);
                }
            }
        }
    }
}