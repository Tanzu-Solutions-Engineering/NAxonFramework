using System;
using System.Runtime.Serialization;
using System.Threading;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling.Saga
{
    public class SagaStorageException : AxonTransientException
    {
        protected SagaStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            
        }

        public SagaStorageException(string message) : base(message)
        {
        }

        public SagaStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}