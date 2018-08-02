using System;
using System.Collections.Async;
using System.Threading;
using System.Threading.Tasks;
using NAxonFramework.Common;
using NAxonFramework.EventHandling;

namespace NAxonFramework.Messaging
{
//    public interface IMessageStream<T> : IMessageStream where T : IMessage
//    {
//        Optional<T> Peek();
//        T NextAvailable();
//    }
//    public interface IMessageStream
//    {
//        bool HasNextAvailable();
//        bool HasNextAvailable(TimeSpan timeout);
//        Optional<IMessage> Peek();
//        IMessage NextAvailable();
//        
//    }


    public interface IMessageStream<T> : IAsyncEnumerator<T>, IMessageStream where T : IMessage
    {
    }

    public interface IMessageStream : IAsyncEnumerator<IMessage>
    {
    }

}