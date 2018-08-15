using System;

namespace NAxonFramework.EventHandling
{
    public interface IListenerInvocationErrorHandler
    {
        void OnError(Exception exception, IEventMessage @event, IEventListener eventListener);
    }
}