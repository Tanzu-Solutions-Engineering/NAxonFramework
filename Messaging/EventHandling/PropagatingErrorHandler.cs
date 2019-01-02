using System;

namespace NAxonFramework.EventHandling
{
    public class PropagatingErrorHandler : IErrorHandler, IListenerInvocationErrorHandler
    {
        private PropagatingErrorHandler()
        {
        }

        public static PropagatingErrorHandler Instance { get; } = new PropagatingErrorHandler();
        public void HandleError(ErrorContext errorContext)
        {
            throw errorContext.Error;
        }

        public void OnError(Exception exception, IEventMessage @event, IEventListener eventListener)
        {
            throw exception;
        }
    }
}