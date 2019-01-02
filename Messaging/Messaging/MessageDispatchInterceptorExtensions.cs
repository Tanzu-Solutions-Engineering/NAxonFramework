using System.Collections.Generic;

namespace NAxonFramework.Messaging
{
    public static class MessageDispatchInterceptorExtensions
    {
        public static T Handle<T>(this IMessageDispatchInterceptor interceptor, T message) where T : IMessage
            => (T)interceptor.Handle(new List<IMessage> {message}).Invoke(0, message);
    }
}