using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling
{
    public class ResetNotSupportedException : AxonNonTransientException
    {
        public ResetNotSupportedException(string message) : base(message)
        {
        }

        public ResetNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ResetNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}