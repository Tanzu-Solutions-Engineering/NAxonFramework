using System;
using System.Collections.Generic;

namespace NAxonFramework.Messaging
{
    public interface ISubscribableMessageSource<M> : IObservable<List<M>> where M : IMessage
    {
        
    }
}