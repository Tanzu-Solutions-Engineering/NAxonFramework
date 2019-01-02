using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common
{
    public class AxonTransientException : AxonException
    {
        protected AxonTransientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AxonTransientException(string message) : base(message)
        {
        }

        public AxonTransientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}