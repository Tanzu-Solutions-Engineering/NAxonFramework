using System;

namespace NAxonFramework.Messaging
{
    public static class IMessageExtensions
    {
        public static Type GetPayloadType<T>(this IMessage<T> message) => message.GetType();
    }
}