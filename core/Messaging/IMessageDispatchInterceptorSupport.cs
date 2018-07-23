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
}