using NAxonFramework.Messaging;

namespace NAxonFramework.Monitoring
{
    public interface IMessageMonitor
    {
        IMonitorCallback OnMessageIngested(IMessage message);
    }
}