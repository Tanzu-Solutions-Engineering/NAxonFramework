using System;
using NAxonFramework.Common;
using NAxonFramework.EventHandling;

namespace NAxonFramework.EventSourcing.EventStore
{
    public interface IEventStore : IEventBus
    {
        IDomainEventStream ReadEvents(string aggregateIdentifier);
        IDomainEventStream ReadEvents(string aggregateIdentifier, long firstSequenceNumber);
        void StoreSnapshot(IDomainEventMessage snapshot);
        Optional<long> LastSequenceNumberFor(string aggregateIdentifier);

    }
}