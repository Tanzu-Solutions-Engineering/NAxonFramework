using System;
using System.Collections.Generic;

namespace NAxonFramework.Messaging
{
    public interface IMessageDispatchInterceptor
    {
        Func<int, IMessage, IMessage> Handle(IList<IMessage> messages);
    }
}