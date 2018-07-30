using System;

namespace NAxonFramework.CommandHandling.Model
{
    public class ConflictingAggregateVersionException : Exception
    {
        public string IdentifierAsString { get; }
        public long ExpectedVersion { get; }
        public long AggregateVersion { get; }

        public ConflictingAggregateVersionException(string identifierAsString, long expectedVersion, long aggregateVersion)
        {
            IdentifierAsString = identifierAsString;
            ExpectedVersion = expectedVersion;
            AggregateVersion = aggregateVersion;
        }
    }
}