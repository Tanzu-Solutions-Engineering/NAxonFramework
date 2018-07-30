using System;
using System.Collections.Generic;

namespace NAxonFramework.CommandHandling.Gateway
{
    public interface IRetryScheduler
    {
        bool ScheduleRetry(ICommandMessage commandMessage, Exception lastFailure, List<Type> failures, Action commandDispatch);
    }
}