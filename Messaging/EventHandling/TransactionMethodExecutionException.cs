using System;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling
{
    public class TransactionMethodExecutionException : AxonException
    {
        public TransactionMethodExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}