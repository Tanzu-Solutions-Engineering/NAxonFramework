using System;

namespace NAxonFramework.CommandHandling.Callbacks
{
    public class NoOpCallback : ICommandCallback<object>
    {
        public static NoOpCallback Instance = new NoOpCallback();
        public void OnSuccess(ICommandMessage commandMessage, object result)
        {
            
        }

        public void OnFailure(ICommandMessage commandMessage, Exception cause)
        {
        }
    }
}