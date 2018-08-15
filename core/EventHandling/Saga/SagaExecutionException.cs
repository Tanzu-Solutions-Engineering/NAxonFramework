using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling.Saga
{
    public class SagaExecutionException : AxonException
    {
        public SagaExecutionException(string message, Exception cause) : base(message, cause)
        {
        }

        protected SagaExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}