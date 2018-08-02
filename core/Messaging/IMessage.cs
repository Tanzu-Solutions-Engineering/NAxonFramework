using System;
using System.Collections.Generic;
using System.Text;

namespace NAxonFramework.Messaging
{
    public interface IMessage<out T> : IMessage
    {
        new T Payload { get; }
    }

    public interface IMessage
    {
        string Identifier { get; }
        MetaData MetaData { get; }
        Type PayloadType { get; }
        object Payload { get; }
        IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData);
        IMessage AndMetaData(IReadOnlyDictionary<string, object> metaData);
    }
}