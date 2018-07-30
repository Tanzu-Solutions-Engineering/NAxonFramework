using System;
using System.Collections.Generic;

namespace NAxonFramework.Messaging
{
    public interface IMessageDispatchInterceptor
    {
        Func<int, IMessage, IMessage> Handle<T>(IList<T> messages) where T : IMessage;
    }
}