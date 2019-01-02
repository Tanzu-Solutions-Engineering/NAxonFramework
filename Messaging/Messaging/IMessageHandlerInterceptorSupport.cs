using System;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.Messaging
{
    public interface IMessageHandlerInterceptorSupport<T>
    {
        IDisposable RegisterHandlerInterceptor(IMessageHandlerInterceptor handlerInterceptor);
    }
}