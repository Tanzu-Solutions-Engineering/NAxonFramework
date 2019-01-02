using System;

namespace NAxonFramework.Monitoring
{
    public class NoOpMessageMonitorCallback : IMonitorCallback
    {
        private NoOpMessageMonitorCallback()
        {
        }

        public static NoOpMessageMonitorCallback Instance { get; } = new NoOpMessageMonitorCallback();
        public void ReportSuccess()
        {
            
        }

        public void ReportFailure(Exception exception)
        {
            
        }

        public void ReportIgnored()
        {
            
        }
    }
}