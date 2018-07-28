using System;
using System.Collections.Generic;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.Messaging
{
    public interface IMessageDispatchInterceptorSupport<T> where T : IMessage
    {
        IDisposable RegisterHandlerInterceptor(IMessageDispatchInterceptor handlerInterceptor);
    }

    public interface IMessageDispatchInterceptor
    {
        Func<int, object, object> Handle(IList<object> messages);
    }

    public static class MessageDispatchInterceptorExtensions
    {
        public static T Handle<T>(this IMessageDispatchInterceptor interceptor, T message)
            => (T)interceptor.Handle(new List<object> {message}).Invoke(0, message);
    }
}