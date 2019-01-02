using Microsoft.Extensions.Logging;
using NAxonFramework.EventHandling;

namespace NAxonFramework.EventSourcing.EventStore
{
    public class EventUtils
    {
        public static ITrackedEventMessage AsTrackedEventMessage(IEventMessage eventMessage, ITrackingToken trackingToken)
        {
            if (eventMessage is IDomainEventMessage)
            {
                return new GenericTrackedDomainEventMessage(trackingToken, (IDomainEventMessage) eventMessage);
            }
            return new GenericTrackedEventMessage(trackingToken, eventMessage);
        }
        
    }
}