using System;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public class DefaultUnitOfWork : AbstractUnitOfWork
    {
        private readonly MessageProcessingContext _processingContext;

        public static DefaultUnitOfWork StartAndGet<T>(T message) where T : IMessage
        {
            var uow = new DefaultUnitOfWork(message);
            uow.Start();
            return uow;
        }

        public DefaultUnitOfWork(IMessage message)
        {
            _processingContext = new MessageProcessingContext(message);
        }

        public override R ExecuteWithResult<R>(Func<R> task, RollbackConfigurationType rollbackConfiguration)
        {
            if (Phase == Phase.NOT_STARTED)
            {
                Start();
            }
            Assert.State(Phase == Phase.STARTED, () => $"The UnitOfWork has an incompatible phase: {Phase}");
            R result;
            try
            {
                result = task();
            }
            catch (Exception e)
            {
                if (rollbackConfiguration.RollbackOn(e))
                {
                    Rollback(e);
                }
                else
                {
                    ExecutionResult = Task.FromException<object>(e);
                    Commit();
                }

                throw;
            }
            ExecutionResult = Task.FromResult((object)result);
            Commit();
            return result;
        }

        protected override void SetRollbackCause(Exception cause)
        {
            ExecutionResult = cause != null ? Task.FromException<object>(cause) : Task.FromCanceled<object>(CancellationToken.None);
        }

        protected override void NotifyHandlers(Phase phase)
        {
            _processingContext.NotifyHandlers(this, phase);
        }

        protected override void AddHandler(Phase phase, Action<IUnitOfWork> handler)
        {
            Assert.State(!phase.IsBefore(Phase), () => $"Cannot register a listener for phase: {phase} because the Unit of Work is already in a later phase: {Phase}");
            _processingContext.AddHandler(phase, handler);
        }

        public override IMessage Message => _processingContext.Message;

        public override IUnitOfWork TransformMessage(Func<IMessage, IMessage> transformOperator)
        {
            _processingContext.TransformMessage<IMessage>(transformOperator);
            return this;
        }

        public override Task<object> ExecutionResult
        {
            get => _processingContext.ExecutionResult;
            set => _processingContext.ExecutionResult = value;
        } 

    }
}