using System.Collections.Generic;
using System.Linq;
using NAxonFramework.CommandHandling.Callbacks;
using NAxonFramework.Common;
using NAxonFramework.Messaging;


namespace NAxonFramework.CommandHandling.Gateway
{
    public abstract class AbstractCommandGateway
    {
        public ICommandBus CommandBus { get; }
        private readonly IRetryScheduler _retryScheduler;
        private readonly List<IMessageDispatchInterceptor> _dispatchInterceptors;
        
        protected AbstractCommandGateway(ICommandBus commandBus, IRetryScheduler retryScheduler,
            IReadOnlyCollection<IMessageDispatchInterceptor> messageDispatchInterceptors) 
        {
            Assert.NotNull(commandBus, () => "commandBus may not be null");
            this.CommandBus = commandBus;
            if (messageDispatchInterceptors != null && messageDispatchInterceptors.Any()) 
            {
                _dispatchInterceptors = new List<IMessageDispatchInterceptor>(messageDispatchInterceptors);
            } 
            else 
            {
                _dispatchInterceptors = new List<IMessageDispatchInterceptor>();
            }
            _retryScheduler = retryScheduler;
        }
        
        protected  void Send<C, R>(C command, ICommandCallback<R> callback) 
        {
            var commandMessage = ProcessInterceptors(GenericCommandMessage<C>.AsCommandMessage(command));
            var commandCallback = callback;
            if (_retryScheduler != null) 
            {
                commandCallback = new RetryingCallback<R>(callback, _retryScheduler, CommandBus);
            }
            CommandBus.Dispatch(commandMessage, commandCallback);
        }
        
        protected void SendAndForget(object command) 
        {
            if (_retryScheduler == null) 
            {
                CommandBus.Dispatch(ProcessInterceptors(GenericCommandMessage<object>.AsCommandMessage(command)));
            } 
            else 
            {
                var commandMessage = GenericCommandMessage<object>.AsCommandMessage(command);
                Send(commandMessage, LoggingCallback.Instance);
            }
        }
        protected ICommandMessage ProcessInterceptors(ICommandMessage commandMessage) 
        {
            var message = commandMessage;
            foreach (var dispatchInterceptor in _dispatchInterceptors) 
            {
                message = dispatchInterceptor.Handle(message);
            }
            return message;
        }
        
    }
}