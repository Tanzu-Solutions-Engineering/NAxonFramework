using System;
using System.Diagnostics.Tracing;

namespace NAxonFramework.EventHandling
{
    public interface IListenerInvocationErrorHandler
    {
        void OnError(Exception exception, IEventMessage @event, EventListener eventListener);
    }
}