using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public interface IEventBus : IObservable<IList<IEventMessage>>, 
        IStreamableMessageSource<ITrackedEventMessage>, 
        IMessageDispatchInterceptorSupport<IEventMessage>
    {
        ITrackingEventStream OpenStream(ITrackingToken trackingToken);
        void Publish(List<IEventMessage> events);
        IDisposable RegisterDispatchInterceptor(IMessageDispatchInterceptor dispatchInterceptor);
    }

    public static class EventBusExtensions
    {
        public static void Publish(this IEventBus eventBus, params IEventMessage[] msg)
        {
            eventBus.Publish(msg.ToList());
        }
    }
}