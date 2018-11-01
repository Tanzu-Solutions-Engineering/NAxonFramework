using System;

namespace NAxonFramework.CommandHandling
{
    public interface ICommandCallback<R> : ICommandCallback
    {
        void OnSuccess(ICommandMessage commandMessage, R result);
    }
    public interface ICommandCallback
    {
        void OnSuccess(ICommandMessage commandMessage, object result);
        void OnFailure(ICommandMessage commandMessage, Exception cause);
    }
}