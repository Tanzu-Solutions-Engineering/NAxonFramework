using System;

namespace NAxonFramework.CommandHandling
{
    public interface ICommandCallback<C,R>
    {
        void OnSuccess(ICommandMessage commandMessage, R result);
        void OnFailure(ICommandMessage commandMessage, Exception cause);
    }
}