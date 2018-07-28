using System;
using NAxonFramework.CommandHandling;

namespace NAxonFramework.Monitoring
{
    public class MonitorAwareCallback<R> : ICommandCallback<R>
    {
        private readonly ICommandCallback<R> _delegate;
        private readonly IMonitorCallback _messageMonitorCallback;

        public MonitorAwareCallback(ICommandCallback<R> @delegate, IMonitorCallback messageMonitorCallback)
        {
            _delegate = @delegate;
            _messageMonitorCallback = messageMonitorCallback;
        }

        public void OnSuccess(ICommandMessage commandMessage, R result)
        {
            _messageMonitorCallback.ReportSuccess();
            if(_delegate != null)
                _delegate.OnSuccess(commandMessage, result);
        }

        public void OnFailure(ICommandMessage commandMessage, Exception cause)
        {
            _messageMonitorCallback.ReportFailure(cause);
            if(_delegate != null)
                _delegate.OnFailure(commandMessage, cause);
            
        }
    }
}