using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NAxonFramework.EventHandling.Saga.MetaModel;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Saga
{
    public class AnnotatedSagaManager<T> : AbstractSagaManager<T>
    {
        private readonly ISagaModel _sagaMetaModel;

        
        public AnnotatedSagaManager(ISagaRepository<T> sagaRepository, Func<T> sagaFactory) 
            : this(sagaRepository, sagaFactory, new AnnotationSagaMetaModelFactory().ModelOf<T>(), new LoggingErrorHandler())
        {
            
        }
        public AnnotatedSagaManager(ISagaRepository<T> sagaRepository, Func<T> sagaFactory, IParameterResolverFactory parameterResolverFactory, IListenerInvocationErrorHandler listenerInvocationErrorHandler) 
            : this(sagaRepository, sagaFactory, new AnnotationSagaMetaModelFactory(parameterResolverFactory).ModelOf<T>(), new LoggingErrorHandler())
        {
            
        }
        public AnnotatedSagaManager(ISagaRepository<T> sagaRepository, Func<T> sagaFactory, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition, IListenerInvocationErrorHandler listenerInvocationErrorHandler) 
            : this(sagaRepository, sagaFactory, new AnnotationSagaMetaModelFactory(parameterResolverFactory, handlerDefinition).ModelOf<T>(), new LoggingErrorHandler())
        {
            
        }
        
        public AnnotatedSagaManager(ISagaRepository<T> sagaRepository, Func<T> sagaFactory, ISagaModel sagaMetaModel, IListenerInvocationErrorHandler listenerInvocationErrorHandler) 
            : base(sagaRepository, sagaFactory, listenerInvocationErrorHandler)
        {
            _sagaMetaModel = sagaMetaModel;
        }

        public override bool CanHandle(IEventMessage eventMessage, Segment segment)
        {
            return _sagaMetaModel.HasHandlerMethod(eventMessage);
        }

        protected override SagaInitializationPolicy GetSagaCreationPolicy(IEventMessage @event)
        {
            var handlers = _sagaMetaModel.FindHandlerMethods(@event);
            foreach (var handler in handlers)
            {
                if (handler.CreationPolicy != SagaCreationPolicy.None)
                {
                    return new SagaInitializationPolicy(handler.CreationPolicy, handler.GetAssociationValue(@event));
                }
            }
            return SagaInitializationPolicy.None;
        }

        protected override HashSet<AssociationValue> ExtractAssociationValues(IEventMessage @event)
        {
            var handlers = _sagaMetaModel.FindHandlerMethods(@event);
            return handlers.Select(handler => handler.GetAssociationValue(@event))
                .Where(x => x != null)
                .ToHashSet();
        }
    }
}