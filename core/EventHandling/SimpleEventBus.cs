using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Monitoring;

namespace NAxonFramework.EventHandling
{
    public class SimpleEventBus : AbstractEventBus
    {
        private ILogger _logger = CommonServiceLocator.ServiceLocator.Current.GetInstance<ILogger<SimpleEventBus>>();
        private const int DEFAULT_QUEUE_CAPACITY = Int32.MaxValue;
        readonly ConcurrentDictionary<EventConsumer, object> _eventStreams = new ConcurrentDictionary<EventConsumer, object>();
        private readonly int _queueCapacity;

        public SimpleEventBus()
        {
            _queueCapacity = DEFAULT_QUEUE_CAPACITY;
        }

        public SimpleEventBus(int queueCapacity, IMessageMonitor messageMonitor) : base(messageMonitor)
        {
            _queueCapacity = queueCapacity;
        }

        protected override void AfterCommit(List<IEventMessage> events)
        {
            _eventStreams.Keys.ForEach(reader => reader.AddEvents(events));
        }

        public override ITrackingEventStream OpenStream(ITrackingToken trackingToken)
        {
            if (trackingToken != null) 
            {
                throw new NotSupportedException("The simple event bus does not support non-null tracking tokens");
            }
            var eventStream = new EventConsumer(_queueCapacity);
            _eventStreams.TryAdd(eventStream, null);
            return eventStream;
        }

        private class EventConsumer : ITrackingEventStream 
        {
            private readonly int _queueCapacity;

            private readonly BlockingQueue<ITrackedEventMessage> _eventQueue;
        private ITrackedEventMessage _peekEvent;

        private EventConsumer(int queueCapacity)
        {
            _queueCapacity = queueCapacity;
            _eventQueue = new BlockingQueue<ITrackedEventMessage>();
        }

        private void AddEvents(List<IEventMessage> events) 
        {
            //add one by one because bulk operations on LinkedBlockingQueues are not thread-safe
            
            events.ForEach(eventMessage => 
            {
                try 
                {
                    _eventQueue.Enqueue(AsTrackedEventMessage(eventMessage, null));
                } 
                catch (ThreadInterruptedException e) 
                {
                    _logger.LogWarning("Event producer thread was interrupted. Shutting down.", e);
                    throw;
                }
            });
        }

        public Optional<ITrackedEventMessage> Peek() 
        {
            return Optional<ITrackedEventMessage>.OfNullable(_peekEvent == null && !HasNextAvailable() ? null : _peekEvent);
        }

        public bool HasNextAvailable(TimeSpan timeout) 
        {
            try 
            {
                return _peekEvent != null || (_peekEvent = _eventQueue.Poll(timeout)) != null;
            } 
            catch (OperationCanceledException e) 
            {
                _logger.LogWarning("Consumer thread was interrupted. Returning thread to event processor.", e);
                Thread.currentThread().interrupt();
                return false;
            }
        }

        @Override
        public TrackedEventMessage<?> nextAvailable() {
            try {
                return peekEvent == null ? eventQueue.take() : peekEvent;
            } catch (InterruptedException e) {
                logger.warn("Consumer thread was interrupted. Returning thread to event processor.", e);
                Thread.currentThread().interrupt();
                return null;
            } finally {
                peekEvent = null;
            }
        }

        @Override
        public void close() {
            eventStreams.remove(this);
        }
    }
        
        
        public override ITrackingToken CreateTailToken()
        {
            throw new NotImplementedException();
        }

        public override ITrackingToken CreateHeadToken()
        {
            throw new NotImplementedException();
        }

        public override ITrackingToken CreateTokenAt(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public override ITrackingToken CreateTokenSince(TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public override IDisposable RegisterHandlerInterceptor(IMessageDispatchInterceptor handlerInterceptor)
        {
            throw new NotImplementedException();
        }
    }
}