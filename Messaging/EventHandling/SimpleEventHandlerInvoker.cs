using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.EventHandling.Async;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling
{
    public class SimpleEventHandlerInvoker : IEventHandlerInvoker
    {
        private readonly List<object> _eventListeners;
        private readonly List<IEventListener> _wrappedEventListeners;
        private readonly IListenerInvocationErrorHandler _listenerInvocationErrorHandler;
        private readonly ISequencingPolicy<IEventMessage> _sequencingPolicy;
        
        private static List<object> DetectList(object[] eventListeners)
        {
            return eventListeners.Length == 1 && (eventListeners[0] is IList list) ? list.Cast<object>().ToList() : eventListeners.ToList();
        }
        public SimpleEventHandlerInvoker(params object[] eventListeners) : this(DetectList(eventListeners), new LoggingErrorHandler()) 
        {
            
        }
        public SimpleEventHandlerInvoker(List<object> eventListeners, IListenerInvocationErrorHandler listenerInvocationErrorHandler)
            : this(eventListeners, listenerInvocationErrorHandler, new SequentialPerAggregatePolicy())
        {
            
        }
        public SimpleEventHandlerInvoker(List<object> eventListeners, IListenerInvocationErrorHandler listenerInvocationErrorHandler,
            ISequencingPolicy<IEventMessage> sequencingPolicy) 
        {
            _eventListeners = eventListeners;
            _wrappedEventListeners = eventListeners
                .Select(listener => listener is IEventListener eventListener ? eventListener : new AnnotationEventListenerAdapter(listener))
                .ToList();
            _sequencingPolicy = sequencingPolicy;
            _listenerInvocationErrorHandler = listenerInvocationErrorHandler;
        }
        public SimpleEventHandlerInvoker(List<object> eventListeners, IParameterResolverFactory parameterResolverFactory, 
            IListenerInvocationErrorHandler listenerInvocationErrorHandler) 
            : this(eventListeners, parameterResolverFactory, listenerInvocationErrorHandler, new SequentialPerAggregatePolicy())
        {
            
        }
        public SimpleEventHandlerInvoker(List<object> eventListeners, IParameterResolverFactory parameterResolverFactory,
            IListenerInvocationErrorHandler listenerInvocationErrorHandler, ISequencingPolicy<IEventMessage> sequencingPolicy) 
        {
            _eventListeners = eventListeners;
            _wrappedEventListeners = eventListeners
                .Select(listener => listener is IEventListener eventListener ? eventListener : new AnnotationEventListenerAdapter(listener))
                .ToList();
            _sequencingPolicy = sequencingPolicy;
            _listenerInvocationErrorHandler = listenerInvocationErrorHandler;
        }

        public IReadOnlyCollection<object> EventListeners => _eventListeners;

        public void Handle(IEventMessage message, Segment segment)
        {
            foreach (var listener in _wrappedEventListeners) 
            {
                try 
                {
                    listener.Handle(message);
                } 
                catch(Exception e) 
                {
                    _listenerInvocationErrorHandler.OnError(e, message, listener);
                }
            }
        }

        public bool CanHandle(IEventMessage eventMessage, Segment segment)
        {
            return HasHandler(eventMessage)
                   && segment.Matches((_sequencingPolicy.GetSequenceIdentifierFor(eventMessage) ?? eventMessage.Identifier).GetHashCode());
        }
        private bool HasHandler(IEventMessage eventMessage) 
        {
            foreach (var eventListener in _wrappedEventListeners) 
            {
                if (eventListener.CanHandle(eventMessage)) 
                {
                    return true;
                }
            }
            return false;
        }
        public bool SupportsReset
        {
            get
            {
                foreach (var eventListener in _wrappedEventListeners) 
                {
                    if (!eventListener.SupportsReset) 
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public void PerformReset()
        {
            _wrappedEventListeners.ForEach(x => x.PrepareReset());
        }
    }
}