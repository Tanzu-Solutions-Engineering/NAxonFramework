using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAxonFramework.CommandHandling.Callbacks;
using NAxonFramework.Common;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Gateway
{
    public class DefaultCommandGateway : AbstractCommandGateway, ICommandGateway
    {

        private ILogger _logger =
            CommonServiceLocator.ServiceLocator.Current.GetInstance<ILogger<DefaultCommandGateway>>();
        
        public DefaultCommandGateway(ICommandBus commandBus, params IMessageDispatchInterceptor[] messageDispatchInterceptors) 
            : base(commandBus, null, messageDispatchInterceptors)
        {
        }
        public DefaultCommandGateway(ICommandBus commandBus, IRetryScheduler retryScheduler, params IMessageDispatchInterceptor[] messageDispatchInterceptors) 
            : base(commandBus, retryScheduler, messageDispatchInterceptors)
        {
        }
        public void Send<C, R>(C command, ICommandCallback<R> callback)
        {
            base.Send(command, callback);
        }

        public Task<R> Send<C, R>(C command)
        {
            var futureCallback = new FutureCallback<R>();
            Send(command, futureCallback);
            return futureCallback.Task; 
        }

        public Task<R> Send<C, R>(C command, TimeSpan timeout)
        {
            return Send<C,R>(command).TimeoutAfter(timeout);
        }
    }
}