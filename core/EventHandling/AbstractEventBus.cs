using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using CommonServiceLocator;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;
using NAxonFramework.Monitoring;

namespace NAxonFramework.EventHandling
{
    public abstract class  AbstractEventBus : IEventBus
    {
        private ILogger _logger = ServiceLocator.Current.GetInstance<ILogger<AbstractEventBus>>();
        private string _eventsKey;
        private IMessageMonitor _messageMonitor;
        private ConcurrentDictionary<IObserver<IList<IEventMessage>>, object> _eventProcessors = new ConcurrentDictionary<IObserver<IList<IEventMessage>>, object>();
        private ConcurrentDictionary<IMessageDispatchInterceptor, object> _dispatchInterceptors = new ConcurrentDictionary<IMessageDispatchInterceptor, object> ();
        
        
        public AbstractEventBus() : this(NoOpMessageMonitor.Instance)
        {
            
        }

        public AbstractEventBus(IMessageMonitor messageMonitor)
        {
            _messageMonitor = messageMonitor;
            _eventsKey = ToString() + "_EVENTS";
        }

        public IDisposable Subscribe(IObserver<IList<IEventMessage>> eventProcessor)
        {
            if (this._eventProcessors.TryAdd(eventProcessor, null)) 
            {
                _logger.LogDebug($"EventProcessor [{eventProcessor}] subscribed successfully");
            } 
            else 
            {
                _logger.LogInformation($"EventProcessor [{eventProcessor}] not added. It was already subscribed");
            }
            return Disposable.Create(() => 
            {
                if (_eventProcessors.TryRemove(eventProcessor, out _)) 
                {
                    _logger.LogDebug($"EventListener {eventProcessor} unsubscribed successfully");
                    
                } 
                else 
                {
                    _logger.LogInformation($"EventListener {eventProcessor} not removed. It was already unsubscribed");
                }
            });
        }

        public IDisposable RegisterDispatchInterceptor(IMessageDispatchInterceptor dispatchInterceptor)
        {
            _dispatchInterceptors.TryAdd(dispatchInterceptor,null);
            return Disposable.Create(() => _dispatchInterceptors.TryRemove(dispatchInterceptor, out _));
        }

        public void Publish(List<IEventMessage> events)
        {
            var ingested = events.Select(x => _messageMonitor.OnMessageIngested(x)).ToList();
            if (CurrentUnitOfWork.IsStarted) 
            {
                var unitOfWork = CurrentUnitOfWork.Get();
                Assert.State(!unitOfWork.Phase.IsAfter(Phase.PREPARE_COMMIT),
                    () => "It is not allowed to publish events when the current Unit of Work has already been " +
                    "committed. Please start a new Unit of Work before publishing events.");
                Assert.State(!unitOfWork.Root().Phase.IsAfter(Phase.PREPARE_COMMIT),
                    () => "It is not allowed to publish events when the root Unit of Work has already been " +
                    "committed.");

                unitOfWork.AfterCommit(u => ingested.ForEach(x => x.ReportSuccess()));
                unitOfWork.OnRollback(u => ingested.ForEach(m => m.ReportFailure(u.ExecutionResult.Exception)));

                EventsQueue(unitOfWork).AddRange(events);
            } 
            else 
            {
                try 
                {
                    PrepareCommit(Intercept(events));
                    Commit(events);
                    AfterCommit(events); 
                    ingested.ForEach(x => x.ReportSuccess());
                } 
                catch (Exception e) 
                {
                    ingested.ForEach(m => m.ReportFailure(e));
                    throw;
                }
            }
        }

        private List<IEventMessage> EventsQueue(IUnitOfWork unitOfWork) 
        {
            return unitOfWork.GetOrComputeResource(_eventsKey, r => 
            {

                var eventQueue = new List<IEventMessage>();

                unitOfWork.OnPrepareCommit(u => 
                {
                    if (u.Parent.IsPresent && !u.Parent.Get().Phase.IsAfter(Phase.COMMIT)) 
                    {
                        EventsQueue(u.Parent.Get()).AddRange(eventQueue);
                    } 
                    else 
                    {
                        int processedItems = eventQueue.Count;
                        DoWithEvents(PrepareCommit, Intercept(eventQueue));
                        // make sure events published during publication prepare commit phase are also published
                        while (processedItems < eventQueue.Count) 
                        {
                            var newMessages = Intercept(eventQueue.GetRange(processedItems, eventQueue.Count));
                            processedItems = eventQueue.Count;
                            DoWithEvents(PrepareCommit, newMessages);
                        }
                    }
                });
                unitOfWork.OnCommit(u => 
                {
                    if (u.Parent.IsPresent && !u.Root().Phase.IsAfter(Phase.COMMIT)) 
                    {
                        u.Root().OnCommit(w => DoWithEvents(Commit, eventQueue));
                    } 
                    else 
                    {
                        DoWithEvents(Commit, eventQueue);
                    }
                });
                unitOfWork.AfterCommit(u => 
                {
                    if (u.Parent.IsPresent && !u.Root().Phase.IsAfter(Phase.AFTER_COMMIT)) 
                    {
                        u.Root().AfterCommit(w => DoWithEvents(AfterCommit, eventQueue));
                    } 
                    else 
                    {
                        DoWithEvents(AfterCommit, eventQueue);
                    }
                });
                unitOfWork.OnCleanup(u => u.Resources.Remove(_eventsKey));
                return eventQueue;

            });
        }

        protected List<IEventMessage> QueuedMessages() 
        {
            if (!CurrentUnitOfWork.IsStarted) 
            {
                return new List<IEventMessage>();
            }
            var messages = new List<IEventMessage>();
            for (var uow = CurrentUnitOfWork.Get(); uow != null; uow = uow.Parent.OrElse(null)) 
            {
                messages.InsertRange(0, uow.GetOrDefaultResource(_eventsKey, new List<IEventMessage>()));
            }
            return messages;
        }
        protected List<IEventMessage> Intercept(List<IEventMessage> events) 
        {
            var preprocessedEvents = new List<IEventMessage>();
            foreach (var preprocessor in _dispatchInterceptors.Keys) 
            {
                var function = preprocessor.Handle(preprocessedEvents);
                for (int i = 0; i < preprocessedEvents.Count; i++) 
                {
                    preprocessedEvents[i] = (IEventMessage)function(i, preprocessedEvents[i]);
                }
            }
            return preprocessedEvents;
        }

        private void DoWithEvents(Action<List<IEventMessage>> eventsConsumer, List<IEventMessage> events)
        {
            DoWithEvents(Observer.Create(eventsConsumer), events);
        }
        private void DoWithEvents(IObserver<List<IEventMessage>> eventsConsumer, List<IEventMessage> events) 
        {
            eventsConsumer.OnNext(events);
        }
        
        protected virtual void PrepareCommit(List<IEventMessage> events) 
        {
            _eventProcessors.Keys.ForEach(eventProcessor => eventProcessor.OnNext(events));
        }
        protected virtual void Commit(List<IEventMessage> events) 
        {
            
        }
        protected virtual void AfterCommit(List<IEventMessage> events) 
        {
        }
        IMessageStream IStreamableMessageSource<ITrackedEventMessage>.OpenStream(ITrackingToken trackingToken)
        {
            return OpenStream(trackingToken);
        }

        public abstract ITrackingEventStream OpenStream(ITrackingToken trackingToken);

        public abstract ITrackingToken CreateTailToken();

        public abstract ITrackingToken CreateHeadToken();

        public abstract ITrackingToken CreateTokenAt(DateTime dateTime);

        public abstract  ITrackingToken CreateTokenSince(TimeSpan duration);

        public abstract IDisposable RegisterHandlerInterceptor(IMessageDispatchInterceptor handlerInterceptor);
    }
}