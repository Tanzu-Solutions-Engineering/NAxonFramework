using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling
{
    public class EventProcessingException : AxonException
    {
        protected EventProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public EventProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}