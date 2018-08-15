using System;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.Common.io;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;
using NAxonFramework.Monitoring;

namespace NAxonFramework.EventHandling
{
    public class SubscribingEventProcessor : AbstractEventProcessor
    {
        private readonly ISubscribableMessageSource<IEventMessage> _messageSource;
        private readonly IEventProcessingStrategy _processingStrategy;
        private IDisposable _eventBusRegistration;
        
        public SubscribingEventProcessor(String name, IEventHandlerInvoker eventHandlerInvoker, ISubscribableMessageSource<IEventMessage> messageSource)
            : this(name, eventHandlerInvoker, messageSource,DirectEventProcessingStrategy.Instance, PropagatingErrorHandler.Instance)
        {
            
        }
        public SubscribingEventProcessor(String name, 
            IEventHandlerInvoker eventHandlerInvoker,
            ISubscribableMessageSource<IEventMessage> messageSource, 
            IEventProcessingStrategy processingStrategy,
            IErrorHandler errorHandler)
            : this(name, eventHandlerInvoker, messageSource, processingStrategy, errorHandler, NoOpMessageMonitor.Instance)
        {
            
        }
        public SubscribingEventProcessor(String name, 
            IEventHandlerInvoker eventHandlerInvoker, 
            ISubscribableMessageSource<IEventMessage> messageSource, 
            IEventProcessingStrategy processingStrategy, 
            IErrorHandler errorHandler,
            IMessageMonitor messageMonitor) 
            : this(name, eventHandlerInvoker, RollbackConfigurationType.AnyThrowable, messageSource, processingStrategy,
                errorHandler, messageMonitor)
        {
            
        }
        
        public SubscribingEventProcessor(String name, 
            IEventHandlerInvoker eventHandlerInvoker,
            RollbackConfigurationType rollbackConfiguration,
            ISubscribableMessageSource<IEventMessage> messageSource, 
            IEventProcessingStrategy processingStrategy, 
            IErrorHandler errorHandler,
            IMessageMonitor messageMonitor)
            : base(name, eventHandlerInvoker, rollbackConfiguration, errorHandler, messageMonitor)
        {
            
            _messageSource = messageSource;
            _processingStrategy = processingStrategy;
        }

        public override void Start()
        {
            if (_eventBusRegistration == null) 
            {
                _eventBusRegistration = _messageSource.Subscribe(eventMessages => _processingStrategy.Handle(eventMessages,Process));
            }
        }
        protected void Process(List<IEventMessage> eventMessages) 
        {
            try 
            {
                ProcessInUnitOfWork(eventMessages, new BatchingUnitOfWork(eventMessages.Cast<IMessage>().ToList()), Segment.RootSegment);
//            } catch (RuntimeException e) {
//                throw e;
            } 
            catch (Exception e) 
            {
                throw new EventProcessingException("Exception occurred while processing events", e);
            }
        }
        public override void Shutdown()
        {
            IOUtils.CloseQuietly(_eventBusRegistration);
            _eventBusRegistration = null;
        }
    }
}