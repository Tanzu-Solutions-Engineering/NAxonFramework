using System;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling
{
    public interface ICommandBus : IMessageHandlerInterceptorSupport<ICommandMessage>, IMessageDispatchInterceptorSupport<ICommandMessage>
    {
        void Dispatch(ICommandMessage command);
        void Dispatch<R>(ICommandMessage command, ICommandCallback<R> callback);
        IDisposable Subscribe(string commandName, IMessageHandler<ICommandMessage> handler);
    }
}