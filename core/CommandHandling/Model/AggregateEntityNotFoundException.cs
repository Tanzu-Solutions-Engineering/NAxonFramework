using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model
{
    public class AggregateEntityNotFoundException : AxonNonTransientException
    {
        public AggregateEntityNotFoundException(string message) : base(message)
        {
        }

        public AggregateEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AggregateEntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}