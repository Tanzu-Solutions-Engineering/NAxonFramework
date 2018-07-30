using System;
using System.Reactive;
using System.Runtime.CompilerServices;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling
{
    public class AnnotationEventListenerAdapter : IEventListenerProxy
    {
        private readonly AnnotatedHandlerInspector _inspector;
        private readonly Type _listenerType;
        private readonly object _annotatedEventListener;

        public AnnotationEventListenerAdapter(object annotatedEventListener) :this(annotatedEventListener, ClasspathParameterResolverFactory.Factory)
        {
            
        }

        private AnnotationEventListenerAdapter(object annotatedEventListener, IParameterResolverFactory parameterResolverFactory) 
            : this(annotatedEventListener, parameterResolverFactory, ClasspathHandlerDefinition.Factory)
        {
        }

        private AnnotationEventListenerAdapter(object annotatedEventListener, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            _annotatedEventListener = annotatedEventListener;
            _listenerType = annotatedEventListener.GetType();
            _inspector = AnnotatedHandlerInspector.InspectType(annotatedEventListener.GetType(), parameterResolverFactory, handlerDefinition); 
        }

        public void Handle(IEventMessage @event)
        {
            foreach (var handler in _inspector.Handlers)
            {
                if (handler.CanHandle(@event))
                {
                    handler.Handle(@event, _annotatedEventListener);
                    break;
                }
            }
        }

        public bool CanHandle(IEventMessage @event)
        {
            foreach (var handler in _inspector.Handlers)
            {
                if (handler.CanHandle(@event))
                {
                    return true;
                }
            }

            return false;
        }

        public Type TargetType => _listenerType;

        public void PrepareReset()
        {
            try 
            {
                Handle(GenericEventMessage<object>.AsEventMessage(new ResetTriggeredEvent()));
            } 
            catch (Exception e) 
            {
                throw new ResetNotSupportedException("An Error occurred while notifying handlers of the reset", e);
            }
        }

        public bool SupportsReset => true;
    }
}