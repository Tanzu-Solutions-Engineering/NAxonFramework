using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

namespace NAxonFramework.EventHandling
{
    public class LoggingErrorHandler : IListenerInvocationErrorHandler 
    {
        private readonly ILogger _logger;

        public LoggingErrorHandler()
        {
            _logger = CommonServiceLocator.ServiceLocator.Current.GetInstance<ILogger<LoggingErrorHandler>>();
        }

        public LoggingErrorHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void OnError(Exception exception, IEventMessage @event, IEventListener eventListener)
        {
            var eventListenerType = eventListener is IEventListenerProxy ? ((IEventListenerProxy) eventListener).TargetType : eventListener.GetType();
            _logger.LogError($"EventListener [{eventListenerType.Name}] failed to handle event [{@event.Identifier}] ({@event.PayloadType}). " +
                         "Continuing processing with next listener", exception);
        }
    }
}