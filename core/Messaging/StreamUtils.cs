using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace NAxonFramework.Messaging
{
    public static class StreamUtils
    {
        public static IObservable<IMessage> ToObservable(this IMessageStream messageStream)
        {
            return Observable.Create<IMessage>(observer =>
            {
                try
                {
                    observer.OnNext(messageStream.NextAvailable());
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
                return Disposable.Empty;
            });
        }
    }
}