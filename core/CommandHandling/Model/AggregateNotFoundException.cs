using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model
{
    public class AggregateNotFoundException : AxonNonTransientException
    {
        public object AggregateIdentifier { get; }

        public AggregateNotFoundException(string aggregateIdentifier, string message) : base(message)
        {
            AggregateIdentifier = aggregateIdentifier;
        }

        public AggregateNotFoundException(string aggregateIdentifier, string message, Exception innerException) : base(message, innerException)
        {
            AggregateIdentifier = aggregateIdentifier;
        }

        protected AggregateNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}