using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Gateway
{
    public class IntervalRetryScheduler : IRetryScheduler
    {
        private readonly int _retryInterval;
        private readonly int _maxRetryCount;
        private readonly ILogger _logger;

        public IntervalRetryScheduler(ILogger logger, int retryInterval, int maxRetryCount)
        {
            _retryInterval = retryInterval;
            _maxRetryCount = maxRetryCount;
            _logger = logger;
        }

        public bool ScheduleRetry(ICommandMessage commandMessage, Exception lastFailure, List<Type> failures, Action dispatchTask)
        {
            int failureCount = failures.Count;
            if (!IsExplicitlyNonTransient(lastFailure) && failureCount <= _maxRetryCount) 
            {
                    _logger.LogInformation($"Processing of Command [{commandMessage.PayloadType.Name}] resulted in an exception. Will retry {_maxRetryCount - failureCount} more time(s)... "
                                + $"Exception was {lastFailure.GetType().Name}, {lastFailure.Message}");
                
                return ScheduleRetry(dispatchTask, _retryInterval);
            } else {
                if (failureCount >= _maxRetryCount) 
                {
                    _logger.LogInformation($"Processing of Command [{commandMessage.PayloadType.Name}] resulted in an exception {failureCount} times. Giving up permanently. ");
                } 
                else
                {
                    _logger.LogInformation($"Processing of Command [{commandMessage.PayloadType.Name}] resulted in an exception and will not be retried. ");
                }
                return false;
            }
        }
        protected bool IsExplicitlyNonTransient(Exception failure) 
        {
            return failure is AxonNonTransientException || (failure.InnerException != null && IsExplicitlyNonTransient(failure.InnerException));
        }
        private bool ScheduleRetry(Action commandDispatch, int interval) 
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(interval); 
                    commandDispatch();
                }
                catch (Exception e)
                {
                 
                }
            });
            return true;
        }
    }
}