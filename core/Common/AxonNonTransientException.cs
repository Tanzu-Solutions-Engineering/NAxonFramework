using System;
using System.Runtime.Serialization;

namespace NAxonFramework.Common
{
    public class AxonNonTransientException : AxonException
    {
        public AxonNonTransientException(string message) : base(message)
        {
        }


        public static bool IsCauseOf(Exception exception)
        {
            return exception != null && (exception is AxonNonTransientException || IsCauseOf(exception.InnerException));
        }
        
        public AxonNonTransientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AxonNonTransientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}