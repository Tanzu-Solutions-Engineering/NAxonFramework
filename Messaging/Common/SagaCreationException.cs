using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common
{
    public class SagaCreationException : AxonNonTransientException
    {
        public SagaCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SagaCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}