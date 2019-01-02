using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging
{
    public class ExecutionException : AxonTransientException
    {
        protected ExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}