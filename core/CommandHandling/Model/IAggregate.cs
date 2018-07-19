using System;

namespace NAxonFramework.CommandHandling.Model
{
    public interface IAggregate<T>
    {
        string Type { get; }
        object Identifier { get; }
        long Version { get; }
        object Handle<T>(ICommandMessage<T> commandMessage);
        R Invoke<R>(Func<T, R> invocation);
        void Execute<T>(Action<T> invocation);
        bool IsDeleted { get; }
        Type RootType { get; }

    }
}