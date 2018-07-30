using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public class BatchingUnitOfWork : AbstractUnitOfWork
    {
        private readonly List<MessageProcessingContext> _processingContexts;
        private MessageProcessingContext _processingContext;

        public BatchingUnitOfWork(List<IMessage> messages) 
        {
            Assert.IsFalse(messages.IsEmpty(), () => "The list of Messages to process is empty");
            _processingContexts = messages.Select(x => new MessageProcessingContext(x)).ToList();
            _processingContext = _processingContexts[0];
        }

        public override R ExecuteWithResult<R>(Func<R> task, RollbackConfigurationType rollbackConfiguration)
        {
            if (Phase == Phase.NOT_STARTED) {
                Start();
            }
            Assert.State(Phase == Phase.STARTED, () => $"The UnitOfWork has an incompatible phase: {Phase}");
            R result = default(R);
            Exception exception = null;
            foreach (var processingContext in _processingContexts) 
            {
                _processingContext = processingContext;
                try 
                {
                    result = task();
                } 
                catch (Exception e) 
                {
                    if (rollbackConfiguration.RollBackOn(e)) 
                    {
                        Rollback(e);
                        throw e;
                    }
                    ExecutionResult = Task.FromException<object>(e);
                    exception = e;
                    continue;
                }
                ExecutionResult = Task.FromResult<object>(result);
            }
            Commit();
            if (exception != null) 
            {
                throw exception;
            }
            return result;
        }

        public override Task<object> ExecutionResult
        {
            get => _processingContext.ExecutionResult;
            set => _processingContext.ExecutionResult = value;
        }
        
        public Dictionary<IMessage, Task<object>> GetExecutionResults() 
        {
            return _processingContexts.ToDictionary(x => x.Message, x => x.ExecutionResult);
        }

        public override IMessage Message => _processingContext.Message;

        public override IUnitOfWork TransformMessage(Func<IMessage, IMessage> transformOperation)
        {
            _processingContext.TransformMessage<IMessage>(transformOperation);
            return this;
        }

        protected override void NotifyHandlers(Phase phase)
        {
            var contexts = phase.IsReverseCallbackOrder() ? _processingContexts.AsEnumerable().Reverse() : _processingContexts;
            contexts.ForEach(context => (_processingContext = context).NotifyHandlers(this, phase));
        }

        protected override void SetRollbackCause(Exception cause)
        {
            _processingContexts.ForEach(context => context.ExecutionResult = Task.FromException<object>(cause));
        }

        protected override void AddHandler(Phase phase, Action<IUnitOfWork> handler)
        {
            _processingContext.AddHandler(phase, handler);
        }

        public bool IsLastMessage() => IsLastMessage(Message);
        public bool IsLastMessage(IMessage message)
        {
            return _processingContexts.Last().Message.Equals(message);
        }
        public bool IsFirstMessage() => IsFirstMessage(Message);
        public bool IsFirstMessage(IMessage message)
        {
            return _processingContexts.First().Message.Equals(message);
        }
    }
}