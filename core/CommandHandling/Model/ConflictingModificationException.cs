using System;
using System.Runtime.Serialization;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model
{
    public class ConflictingModificationException : AxonNonTransientException
    {
        public ConflictingModificationException(string message) : base(message)
        {
        }

        public ConflictingModificationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConflictingModificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}