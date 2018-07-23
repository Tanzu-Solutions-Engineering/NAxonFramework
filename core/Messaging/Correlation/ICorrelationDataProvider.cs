using System.Collections.Generic;

namespace NAxonFramework.Messaging.Correlation
{
    public interface ICorrelationDataProvider
    {
        IReadOnlyDictionary<string, T> CorrelationDataFor<T>(IMessage<T> message);
    }
}