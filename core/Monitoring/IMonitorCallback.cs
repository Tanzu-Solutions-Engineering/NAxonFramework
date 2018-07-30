using System;

namespace NAxonFramework.Monitoring
{
    public interface IMonitorCallback
    {
        void ReportSuccess();
        void ReportFailure(Exception exception);
        void ReportIgnored();
    }
}