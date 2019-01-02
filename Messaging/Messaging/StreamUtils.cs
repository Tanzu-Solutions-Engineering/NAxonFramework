using System;
using System.Collections.Async;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NAxonFramework.Messaging
{
    public static class StreamUtils
    {
        public static IObservable<T> ToObservable<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            
            return Observable.Create<T>(async observer =>
            {
                var enumerator = await asyncEnumerable.GetAsyncEnumeratorAsync();
                var cancellationDisposable = new CancellationDisposable();

                ReadAndPublishAsync(enumerator, observer, cancellationDisposable.Token);
                
                
                return cancellationDisposable;
            });
        }

        private static async Task ReadAndPublishAsync<T>(IAsyncEnumerator<T> enumerator, IObserver<T> observer, CancellationToken cancellationToken)
        {
            try
            {
                while (await enumerator.MoveNextAsync(cancellationToken))
                {
                    observer.OnNext(enumerator.Current);
                }
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
            finally
            {
                observer.OnCompleted();
            }
        }
    }
    
}