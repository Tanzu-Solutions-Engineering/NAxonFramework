using System;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging
{
    public interface IMessageStream<T> : IMessageStream where T : IMessage
    {
        Optional<T> Peek();
        T NextAvailable();
    }
    public interface IMessageStream
    {
        bool HasNextAvailable();
        bool HasNextAvailable(TimeSpan timeout);
        Optional<IMessage> Peek();
        IMessage NextAvailable();
        
    }
}