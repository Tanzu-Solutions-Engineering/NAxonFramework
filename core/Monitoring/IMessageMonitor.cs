using System;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.Messaging;

namespace NAxonFramework.Monitoring
{
    public interface IMessageMonitor
    {
        IMonitorCallback OnMessageIngested(IMessage message);
    }

    public static class IMessageMonitorExtensions
    {
        public static IReadOnlyDictionary<IMessage, IMonitorCallback> OnMessageIngested(this IMessageMonitor monitor, ICollection<IMessage> messages) 
            => messages.ToDictionary(x => x, monitor.OnMessageIngested);
    }

    public interface IMonitorCallback
    {
        void ReportSuccess();
        void ReportFailure(Exception exception);
        void ReportIgnored();
    }
}