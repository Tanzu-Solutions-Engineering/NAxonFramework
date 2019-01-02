using System.Collections.Generic;
using NAxonFramework.EventSourcing;

namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public class DefaultConflictDescription : IConflictDescription
    {
        public DefaultConflictDescription(string aggregateIdentifier, long expectedVersion, long actualVersion, List<IDomainEventMessage> unexpectedEvents)
        {
            AggregateIdentifier = aggregateIdentifier;
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
            UnexpectedEvents = unexpectedEvents;
        }

        public string AggregateIdentifier { get; }
        public long ExpectedVersion { get; }
        public long ActualVersion { get; }
        public List<IDomainEventMessage> UnexpectedEvents { get; }
    }
}