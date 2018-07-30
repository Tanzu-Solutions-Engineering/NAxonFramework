using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public interface ITrackingEventStream : IMessageStream<ITrackedEventMessage>
    {
    }
}