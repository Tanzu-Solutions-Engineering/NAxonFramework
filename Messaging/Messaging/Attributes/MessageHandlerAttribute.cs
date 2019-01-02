using System;

namespace NAxonFramework.Messaging.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method)]
    public class MessageHandlerAttribute : Attribute
    {
        public MessageHandlerAttribute(Type messageType = null, Type payloadType = null)
        {
            MessageType = messageType ?? typeof(object);
            PayloadType = payloadType ?? typeof(object);
        }

        public Type MessageType { get; } 
        public Type PayloadType { get; } 
    }
}