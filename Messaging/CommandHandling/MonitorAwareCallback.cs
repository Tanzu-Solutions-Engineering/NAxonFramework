using System;
using NAxonFramework.Monitoring;

namespace NAxonFramework.CommandHandling
{
    public class MonitorAwareCallback : ICommandCallback
    {
        private readonly ICommandCallback _delegate;
        private readonly IMonitorCallback _messageMonitorCallback;

        public MonitorAwareCallback(ICommandCallback @delegate, IMonitorCallback messageMonitorCallback)
        {
            _delegate = @delegate;
            _messageMonitorCallback = messageMonitorCallback;
        }

        public void OnSuccess(ICommandMessage commandMessage, object result)
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