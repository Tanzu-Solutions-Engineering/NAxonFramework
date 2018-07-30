using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common.Lock
{
    public class LockAcquisitionFailedException : AxonException
    {
        protected LockAcquisitionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public LockAcquisitionFailedException(string message) : base(message)
        {
        }

        public LockAcquisitionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}