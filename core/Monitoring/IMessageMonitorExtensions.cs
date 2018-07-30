using System.Collections.Generic;
using System.Linq;
using NAxonFramework.Messaging;

namespace NAxonFramework.Monitoring
{
    public static class IMessageMonitorExtensions
    {
        public static IReadOnlyDictionary<IMessage, IMonitorCallback> OnMessageIngested(this IMessageMonitor monitor, ICollection<IMessage> messages) 
            => messages.ToDictionary(x => x, monitor.OnMessageIngested);
    }
}