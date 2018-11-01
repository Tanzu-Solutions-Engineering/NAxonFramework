using System;

namespace NAxonFramework.CommandHandling.Model
{
    public interface IAggregate
    {
        string Type { get; }
        object Identifier { get; }
        long? Version { get; }

        bool IsDeleted { get; }
        Type RootType { get; }
    }

    public interface IAggregate<T> : IAggregate
    {
        object Handle(ICommandMessage commandMessage);
        R Invoke<R>(Func<T, R> invocation);
        void Execute(Action<T> invocation);
    }
}