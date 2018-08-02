using System;
using System.Collections.Async;
using System.Threading;
using System.Threading.Tasks;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public class TrackingEventStream : ITrackingEventStream
    {
        private AsyncEnumerator<ITrackedEventMessage> _enumerator;

        public TrackingEventStream(Func<AsyncEnumerator<ITrackedEventMessage>.Yield,Task> action, IDisposable disposeAction = null)
        {
            _enumerator = new AsyncEnumerator<ITrackedEventMessage>(action);
        }


        public void Dispose() => _enumerator.Dispose();

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken)) => _enumerator.MoveNextAsync(cancellationToken);
        IMessage IAsyncEnumerator<IMessage>.Current => Current;

        public ITrackedEventMessage Current => _enumerator.Current;

        object IAsyncEnumerator.Current => Current;

       
    }
}