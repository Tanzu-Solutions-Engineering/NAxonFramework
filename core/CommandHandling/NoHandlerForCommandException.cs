using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling
{
    public class NoHandlerForCommandException : AxonNonTransientException
    {
        public NoHandlerForCommandException(string message) : base(message)
        {
        }

        public NoHandlerForCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoHandlerForCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}