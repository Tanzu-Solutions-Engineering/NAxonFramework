using NAxonFramework.EventSourcing;

namespace NAxonFramework.EventHandling.Async
{
    public class SequentialPerAggregatePolicy : ISequencingPolicy<IEventMessage>
    {
        public object GetSequenceIdentifierFor(IEventMessage @event)
        {
            if (@event is IDomainEventMessage domainEvent)
            {
                return domainEvent.AggregateIdentifier;
            }
            return null;
        }
    }
}