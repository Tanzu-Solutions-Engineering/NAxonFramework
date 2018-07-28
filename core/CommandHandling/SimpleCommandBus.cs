using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Transactions;
using Microsoft.Extensions.Logging;
using NAxonFramework.CommandHandling.Callbacks;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;
using NAxonFramework.Monitoring;

namespace NAxonFramework.CommandHandling
{
    public class SimpleCommandBus : ICommandBus
    {
        private readonly ILogger<SimpleCommandBus> _logger;
        private readonly ConcurrentDictionary<string, IMessageHandler<ICommandMessage>> _subscriptions = new ConcurrentDictionary<string, IMessageHandler<ICommandMessage>>();
        private readonly ConcurrentDictionary<IMessageHandlerInterceptor,object> _handlerInterceptors = new ConcurrentDictionary<IMessageHandlerInterceptor,object>();
        private readonly ConcurrentDictionary<IMessageDispatchInterceptor,object> _dispatchInterceptors = new ConcurrentDictionary<IMessageDispatchInterceptor,object>();
        private readonly IMessageMonitor _messageMonitor;
        public RollbackConfigurationType RollbackConfiguration { get; } = RollbackConfigurationType.UncheckedException;


        public SimpleCommandBus(ILogger<SimpleCommandBus> logger, IMessageMonitor messageMonitor)
        {
            _logger = logger;
            _messageMonitor = messageMonitor;
        }

        public void Dispatch<C, R>(ICommandMessage<C> command, ICommandCallback<R> callback)
        {
            DoDisptach(Intercept(command), callback);
        }

        private ICommandMessage<C> Intercept<C>(ICommandMessage<C> command)
        {
            var commandToDispatch = command;
            foreach (var interceptor in _dispatchInterceptors.Keys)
            {
                commandToDispatch = interceptor.Handle(commandToDispatch);
            }

            return commandToDispatch;
        }

        private void DoDisptach<C, R>(ICommandMessage<C> command, ICommandCallback<R> callback)
        {
            var monitorCallback = _messageMonitor.OnMessageIngested(command);
            var handler = FindCommandHandlerFor(command).OrElseThrow(() =>
            {
                var exception = new NoHandlerForCommandException($"No handler was subscribed to command {command.CommandName}");
                monitorCallback.ReportFailure(exception);
                return exception;
            });
            Handle(command, handler, new MonitorAwareCallback<R>(callback, monitorCallback));
        }
        private Optional<IMessageHandler<ICommandMessage>> FindCommandHandlerFor(ICommandMessage command) {
            return Optional<IMessageHandler<ICommandMessage>>.OfNullable(_subscriptions.GetValueOrDefault(command.CommandName));
        }

        protected void Handle<R>(ICommandMessage command, IMessageHandler<ICommandMessage> handler, ICommandCallback<R> callback)
        {
            _logger.LogDebug($"Handling command [{command.CommandName}]");
            try
            {
                var unitOfWork = DefaultUnitOfWork.StartAndGet(command);
                // no need to attach transaction - ambient
                var chain = new DefaultInterceptorChain(unitOfWork, _handlerInterceptors.Keys.GetEnumerator(), handler);
                var result = unitOfWork.ExecuteWithResult(chain.Proceed, RollbackConfiguration);
                callback.OnSuccess(command,(R)result); // todo: double check R cast
            }
            catch (Exception e)
            {
                callback.OnFailure(command, e);
            }
        }

        public IDisposable Subscribe(string commandName, IMessageHandler<ICommandMessage> handler)
        {
            _subscriptions[commandName] = handler;
            return Disposable.Create(() =>
            {
                if (_subscriptions.TryGetValue(commandName, out var value) && handler == value)
                    _subscriptions.TryRemove(commandName, out _);
            });
        }

        public IDisposable RegisterHandlerInterceptor(IMessageHandlerInterceptor handlerInterceptor)
        {
            _handlerInterceptors.TryAdd(handlerInterceptor, null);
            return Disposable.Create(() => _handlerInterceptors.TryRemove(handlerInterceptor, out _));
        }

        public IDisposable RegisterHandlerInterceptor(IMessageDispatchInterceptor dispatchInterceptor)
        {
            _dispatchInterceptors.TryAdd(dispatchInterceptor, null);
            return Disposable.Create(() => _dispatchInterceptors.TryRemove(dispatchInterceptor, out _));
        }

        public void Dispatch<C>(ICommandMessage<C> command)
        {
            Dispatch(command, LoggingCallback.Instance);
        }
    }
}