using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model
{
    public class AggregateInvocationException : AxonException
    {
        protected AggregateInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AggregateInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}