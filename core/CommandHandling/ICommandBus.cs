using System;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling
{
    public interface ICommandBus : IMessageHandlerInterceptorSupport<ICommandMessage>, IMessageDispatchInterceptorSupport<ICommandMessage>
    {
        void Dispatch<C>(ICommandMessage<C> command);
        void Dispatch<C,R>(ICommandMessage<C> command, ICommandCallback<C,R> callback);
        IDisposable Subscribe(string commandName, IMessageHandler<ICommandMessage> handler);
    }
}