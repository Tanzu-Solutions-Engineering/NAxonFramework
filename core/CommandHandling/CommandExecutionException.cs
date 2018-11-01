using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling
{
    public class CommandExecutionException : AxonException
    {
        protected CommandExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CommandExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}