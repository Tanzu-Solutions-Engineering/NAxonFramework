using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common
{
    public class AxonConfigurationException : AxonNonTransientException
    {

        public AxonConfigurationException(string message) : base(message)
        {
        }

        public AxonConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AxonConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}