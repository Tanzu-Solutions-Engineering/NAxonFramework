using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common
{
    public class AxonException : ApplicationException
    {
        protected AxonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AxonException(string message) : base(message)
        {
        }

        public AxonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}