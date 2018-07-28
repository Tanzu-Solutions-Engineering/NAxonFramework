using System.Collections.Generic;

namespace NAxonFramework.Messaging.Correlation
{
    public interface ICorrelationDataProvider
    {
        IReadOnlyDictionary<string, object> CorrelationDataFor(IMessage message);
    }
}