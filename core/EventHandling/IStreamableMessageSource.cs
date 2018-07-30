using System;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public interface IStreamableMessageSource<T> where T : IMessage
    {
        IMessageStream OpenStream(ITrackingToken trackingToken);
        ITrackingToken CreateTailToken();
        ITrackingToken CreateHeadToken();
        ITrackingToken CreateTokenAt(DateTime dateTime);
        ITrackingToken CreateTokenSince(TimeSpan duration);
    }
}