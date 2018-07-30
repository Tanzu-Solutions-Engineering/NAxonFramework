namespace NAxonFramework.EventHandling
{
    public interface ITrackedEventMessage<T> : ITrackedEventMessage, IEventMessage<T>
    {
    }
}