using System;
using System.Collections.Generic;
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


    public interface ITrackedEventMessage : IEventMessage
    {
        ITrackingToken TrackingToken { get; }
    }
}