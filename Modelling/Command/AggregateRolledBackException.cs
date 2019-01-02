using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model
{
    public class AggregateRolledBackException : AxonException
    {
        public string AggregateIdentifier { get; }

        protected AggregateRolledBackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AggregateRolledBackException(string aggregateIdentifier) 
            : base($"Aggregate with id {aggregateIdentifier} was potentially modified in a Unit of Work that was " +
                   "rolled back. Saving its current state is unsafe.")
        {
            AggregateIdentifier = aggregateIdentifier;
        }


    }
}