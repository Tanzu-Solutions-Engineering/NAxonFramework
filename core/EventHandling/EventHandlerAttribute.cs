using System;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling
{
    [MessageHandler(messageType: typeof(IEventMessage))]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EventHandlerAttribute : Attribute
    {
        public EventHandlerAttribute(Type payloadType = null)
        {
            PayloadType = payloadType ?? typeof(object);
        }

        public Type PayloadType { get; }
    }
    
}