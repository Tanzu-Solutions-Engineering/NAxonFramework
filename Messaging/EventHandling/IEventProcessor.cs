using System;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.EventHandling
{
    public interface IEventProcessor
    {
        string Name { get; }
        IDisposable RegisterInterceptor(IMessageHandlerInterceptor interceptor);
        void Start();
        void Shutdown();
    }
}