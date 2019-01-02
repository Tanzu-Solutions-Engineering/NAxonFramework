using NAxonFramework.Messaging;

namespace NAxonFramework.Monitoring
{
    public class NoOpMessageMonitor : IMessageMonitor
    {
        public static NoOpMessageMonitor Instance { get; } = new NoOpMessageMonitor();
        private NoOpMessageMonitor()
        {
        }

        public IMonitorCallback OnMessageIngested(IMessage message)
        {
            return NoOpMessageMonitorCallback.Instance;
        }
    }
}