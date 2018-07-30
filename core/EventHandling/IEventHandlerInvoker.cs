namespace NAxonFramework.EventHandling
{
    public interface IEventHandlerInvoker
    {
        bool CanHandle(IEventMessage eventMessage, Segment segment);
        void Handle(IEventMessage message, Segment segment);
        bool SupportsReset { get; }
        void PerformReset();
    }
}