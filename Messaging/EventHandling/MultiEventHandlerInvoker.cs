using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;

namespace NAxonFramework.EventHandling
{
    public class MultiEventHandlerInvoker : IEventHandlerInvoker
    {
        private readonly List<IEventHandlerInvoker> _delegates;

        public MultiEventHandlerInvoker(params IEventHandlerInvoker[] delegates) : this(delegates.ToList())
        {
            
        }
        
        public MultiEventHandlerInvoker(List<IEventHandlerInvoker> delegates)
        {
            _delegates = delegates;
        }
        private List<IEventHandlerInvoker> Flatten(List<IEventHandlerInvoker> invokers) 
        {
            var flattened = new List<IEventHandlerInvoker>();
            foreach (var invoker in invokers) 
            {
                if (invoker is MultiEventHandlerInvoker) 
                {
                    flattened.AddRange(((MultiEventHandlerInvoker) invoker).Delegates);
                } 
                else 
                {
                    flattened.Add(invoker);
                }
            }
            return flattened;
        }
        public bool CanHandle(IEventMessage eventMessage, Segment segment)
        {
            return _delegates.Any(x => x.CanHandle(eventMessage, segment));
        }

        public void Handle(IEventMessage message, Segment segment)
        {
            _delegates.Where(x => x.CanHandle(message, segment)).ForEach(i =>
            {
                try
                {
                    i.Handle(message,segment);
                }
                catch (Exception e)
                {
                }
            });
        }

        public bool SupportsReset => true;

        public List<IEventHandlerInvoker> Delegates => _delegates.ToList();

        public void PerformReset()
        {
        }
    }
}