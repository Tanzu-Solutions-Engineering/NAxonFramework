using System;
using System.Threading.Tasks;

namespace NAxonFramework.CommandHandling.Callbacks
{
    public class FutureCallback<R> : ICommandCallback<R>
    {
        
        private TaskCompletionSource<R> _tcs;
        public void OnSuccess(ICommandMessage commandMessage, R result)
        {
            _tcs.SetResult(result);
        }

        public void OnFailure(ICommandMessage commandMessage, Exception cause)
        {
            if (cause == null) throw new ArgumentNullException(nameof(cause));
            _tcs.SetException(cause);
        }
        public Task<R> Task => _tcs.Task;
    }
}