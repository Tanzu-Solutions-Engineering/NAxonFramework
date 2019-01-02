using System;
using System.Collections.Generic;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public interface IEventMessage<T> : IMessage<T>, IEventMessage
    {
//        IEventMessage<T> WithMetaData<K>(IDictionary<string, K> metaData);
//        IEventMessage<T> AndMetaData<K>(IDictionary<string, K> metaData);
    }

    public interface IEventMessage : IMessage
    {
        DateTime Timestamp { get; }
    }


}