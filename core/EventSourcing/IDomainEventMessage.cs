using NAxonFramework.EventHandling;

namespace NAxonFramework.EventSourcing
{
    public interface IDomainEventMessage<T> : IEventMessage<T>, IDomainEventMessage
    {
        
    }
    public interface IDomainEventMessage : IEventMessage
    {
        long SequenceNumber { get; }
        string AggregateIdentifier { get; }
        string Type { get; }
        
    }
}