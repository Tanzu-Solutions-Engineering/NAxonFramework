using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Saga.MetaModel
{
    public class AnnotationSagaMetaModelFactory : ISagaMetaModelFactory
    {
        private readonly ConcurrentDictionary<Type, ISagaModel> _registry = new ConcurrentDictionary<Type, ISagaModel>();

        private readonly IParameterResolverFactory _parameterResolverFactory;
        private readonly IHandlerDefinition _handlerDefinition;

        public AnnotationSagaMetaModelFactory() : this(ClasspathParameterResolverFactory.Factory)
        {
        }

        public AnnotationSagaMetaModelFactory(IParameterResolverFactory parameterResolverFactory) : this(parameterResolverFactory, ClasspathHandlerDefinition.Factory)
        {
        }

        public AnnotationSagaMetaModelFactory(IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            _parameterResolverFactory = parameterResolverFactory;
            _handlerDefinition = handlerDefinition;
        }

        public ISagaModel ModelOf<T>()
        {
            return _registry.ComputeIfAbsent(typeof(T), DoCreateModel);
        }

        private ISagaModel DoCreateModel(Type sagaType)
        {
            var handlerInspector =
                AnnotatedHandlerInspector.InspectType(sagaType,
                    _parameterResolverFactory,
                    _handlerDefinition);

            return new InspectedSagaModel(handlerInspector.Handlers, this);
        }
        private class InspectedSagaModel : ISagaModel
        {
            private readonly List<IMessageHandlingMember> _handlers;
            private readonly AnnotationSagaMetaModelFactory _parent;

            public InspectedSagaModel(List<IMessageHandlingMember> handlers, AnnotationSagaMetaModelFactory parent)
            {
                _handlers = handlers;
                _parent = parent;
            }

            public Optional<AssociationValue> ResolveAssociation(IEventMessage eventMessage)
            {
                foreach (var  handler in _handlers) 
                {
                    if (handler.CanHandle(eventMessage)) {
                        return handler.Unwrap<SagaMethodMessageHandlingMember>()
                            .Map(mh => mh.GetAssociationValue(eventMessage));
                    }
                }
                return Optional<AssociationValue>.Empty;
            }

            public List<SagaMethodMessageHandlingMember> FindHandlerMethods(IEventMessage @event)
            {
                return _handlers.Where(h => h.CanHandle(@event))
                    .Select(h => h.Unwrap<SagaMethodMessageHandlingMember>().OrElse(null))
                    .Where(x => x != null)
                    .ToList();
            }

            public bool HasHandlerMethod(IEventMessage eventMessage)
            {
                foreach (var handler in _handlers) 
                {
                    if (handler.CanHandle(eventMessage)) 
                    {
                        return true;
                    }
                }
                return false;
            }

            public ISagaMetaModelFactory ModelFactory => _parent;
        }
    }
}