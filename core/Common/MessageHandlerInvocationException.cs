using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common
{
    public class MessageHandlerInvocationException : AxonException
    {
        protected MessageHandlerInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }


        public MessageHandlerInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}