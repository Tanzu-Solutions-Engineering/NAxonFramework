using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common.Lock
{
    public class DeadlockException : LockAcquisitionFailedException
    {
        protected DeadlockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DeadlockException(string message) : base(message)
        {
        }

        public DeadlockException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}