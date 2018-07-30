using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net.Mime;
using NAxonFramework.Common.Lock;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling.Gateway
{
    public class RetryingCallback<R> : ICommandCallback<R>
    {
        private readonly ICommandCallback<R> _delegate;
        private readonly IRetryScheduler _retryScheduler;
        private readonly ICommandBus _commandBus;
        private readonly List<Type> _history;

        public RetryingCallback(ICommandCallback<R> @delegate, IRetryScheduler retryScheduler, ICommandBus commandBus) 
        {
            _delegate = @delegate;
            _retryScheduler = retryScheduler;
            _commandBus = commandBus;
            _history = new List<Type>();
        }

        public void OnSuccess(ICommandMessage commandMessage, R result)
        {
            _delegate.OnSuccess(commandMessage, result);
        }

        public void OnFailure(ICommandMessage commandMessage, Exception cause)
        {
            
            _history.AddRange(Simplify(cause));
            try 
            {
                //todo: examine the extra logic here especially around checked exceptions & deadlocks on how it maps to .net
                // we fail immediately when the exception is checked,
                // or when it is a Deadlock Exception and we have an active unit of work
                if ((IsCausedBy(cause, typeof(DeadlockException)) && CurrentUnitOfWork.IsStarted)
                    || !_retryScheduler.ScheduleRetry(commandMessage, cause, new List<Type>(_history), () => RetryDispatch(commandMessage))) 
                {
                    _delegate.OnFailure(commandMessage, cause);
                }
            } 
            catch (Exception e) 
            {
                _delegate.OnFailure(commandMessage, e);
            }
        }

        private bool IsCausedBy(Exception exception, Type causeType)
        {
            return causeType.IsInstanceOfType(exception) 
                   || (exception.InnerException != null && IsCausedBy(exception.InnerException, causeType));
        }

        private void RetryDispatch(ICommandMessage commandMessage)
        {
            try
            {
                _commandBus.Dispatch(commandMessage, this);
            }
            catch (Exception e)
            {
                OnFailure(commandMessage, e);
            }
        }
        
        private Type[] Simplify(Exception cause) {
            var types = new List<Type>();
            types.Add(cause.GetType());
            Exception rootCause = cause;
            while (rootCause.InnerException != null) 
            {
                rootCause = rootCause.InnerException;
                types.Add(rootCause.GetType());
            }
            return types.ToArray();
        }      
    }
}