using System;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling
{
    public interface ICommandBus : IMessageHandlerInterceptorSupport<ICommandMessage>, IMessageDispatchInterceptorSupport<ICommandMessage>
    {
        void Dispatch(ICommandMessage command);
        void Dispatch(ICommandMessage command, ICommandCallback callback);
        IDisposable Subscribe(string commandName, IMessageHandler<ICommandMessage> handler);
    }
}