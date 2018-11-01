using System.Collections.Generic;

namespace NAxonFramework.EventSourcing.EventStore
{
    public interface IDomainEventStream : IEnumerable<IDomainEventMessage>
    {
//        IDomainEventMessage Peek();
//        long LastSequenceNumber { get; }
        
    }
}