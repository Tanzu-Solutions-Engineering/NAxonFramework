using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonServiceLocator;
using Microsoft.Extensions.Logging;
using NAxonFramework.Common;
using Nito.Collections;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public class MessageProcessingContext
    {
        private readonly ILogger<MessageProcessingContext> _logger = ServiceLocator.Current.GetInstance<ILogger<MessageProcessingContext>>();
        private readonly Dictionary<Phase, Deque<Action<IUnitOfWork>>> _handlers = new Dictionary<Phase, Deque<Action<IUnitOfWork>>>();
        public IMessage Message { get; private set; }
        public Task<object> ExecutionResult { get; set; }

        public MessageProcessingContext(IMessage message)
        {
            Message = message;
        }

        public void NotifyHandlers(IUnitOfWork unitOfWork, Phase phase)
        {
            _logger.LogDebug($"Notifying handlers for phase {phase}");
            if (!_handlers.TryGetValue(phase, out var l))
                return;
            while (l.Any())
            {
                l.RemoveFromFront().Invoke(unitOfWork);
            }
        }

        public void AddHandler(Phase phase, Action<IUnitOfWork> handler)
        {
            _logger.LogDebug($"Adding handler {handler.Method?.DeclaringType?.Name} for phase {phase}");
            var consumers = _handlers.ComputeIfAbsent(phase, p => new Deque<Action<IUnitOfWork>>());
            if (phase.IsReverseCallbackOrder())
            {
                consumers.AddToFront(handler);
            }
            else
            {
                consumers.AddToBack(handler);
            }
        }

//        public void SetExectuionResult(Task<object> executionResult)
//        {
////            if(this._executionResult == null || _executionResult.IsFaulted)
////                throw new InvalidOperationException($"Cannot change execution result {_message} to {(_executionResult.IsFaulted ? _executionResult.Exception : _executionResult.Result)} for message [%s].");
//            this.ExecutionResult = executionResult;
//        }

        public void TransformMessage<T>(Func<IMessage, IMessage> transformOperation)
        {
            Message = transformOperation(Message);
        }

        public void Reset(IMessage message)
        {
            Message = message;
            _handlers.Clear();
            ExecutionResult = null;
        }
    }
}