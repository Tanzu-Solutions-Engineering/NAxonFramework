using System;
using System.Collections.Generic;
using System.Text;

namespace NAxonFramework.Messaging
{
    public interface IMessage<out T> : IMessage
    {
        new T Payload { get; }
        IMessage<T> WithMetaData(IReadOnlyDictionary<string, object> metaData);
        IMessage<T> AndMetaData(IReadOnlyDictionary<string, object> metaData);


    }

    public interface IMessage
    {
        string Identifier { get; }
        MetaData MetaData { get; }
        Type PayloadType { get; }
        object Payload { get; }
    }


    public static class IMessageExtensions
    {
        public static Type GetPayloadType<T>(this IMessage<T> message) => message.GetType();
    }
    
}