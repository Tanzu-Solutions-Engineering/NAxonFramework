using System.Collections.Generic;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling
{
    public interface ICommandMessage<out T> : IMessage<T>
    {
        string CommandName { get; }
        new ICommandMessage<T> WithMetaData(IReadOnlyDictionary<string, object> metaData);
        new ICommandMessage<T> AndMetaData(IReadOnlyDictionary<string, object> metaData);

    }
}