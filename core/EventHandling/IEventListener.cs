namespace NAxonFramework.EventHandling
{
    public interface IEventListener
    {
        void Handle(IEventMessage @event);
        bool CanHandle(IEventMessage @event);
        void PrepareReset();
        bool SupportsReset { get; }
    }
}