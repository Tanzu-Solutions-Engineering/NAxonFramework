using System;

namespace NAxonFramework.CommandHandling
{
    public interface ICommandCallback<R>
    {
        void OnSuccess(ICommandMessage commandMessage, R result);
        void OnFailure(ICommandMessage commandMessage, Exception cause);
    }
}