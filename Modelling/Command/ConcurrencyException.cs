using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model
{
    public class ConcurrencyException : AxonTransientException
    {
        protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ConcurrencyException(string message) : base(message)
        {
        }

        public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}