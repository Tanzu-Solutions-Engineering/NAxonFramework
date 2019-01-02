namespace NAxonFramework.EventHandling
{
    public interface ITrackedEventMessage<T> : ITrackedEventMessage, IEventMessage<T>
    {
    }
    public interface ITrackedEventMessage : IEventMessage
    {
        ITrackingToken TrackingToken { get; }
    }
}