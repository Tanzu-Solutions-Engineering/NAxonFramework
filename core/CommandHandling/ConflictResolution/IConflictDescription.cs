using System.Collections.Generic;
using NAxonFramework.EventSourcing;

namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public interface IConflictDescription
    {
        string AggregateIdentifier { get; }
        long ExpectedVersion { get; }
        long ActualVersion { get; }
        List<IDomainEventMessage> UnexpectedEvents { get; }
    }
}