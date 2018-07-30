using System;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.Messaging
{
    public interface IMessageDispatchInterceptorSupport<T> where T : IMessage
    {
        IDisposable RegisterHandlerInterceptor(IMessageDispatchInterceptor handlerInterceptor);
    }
}