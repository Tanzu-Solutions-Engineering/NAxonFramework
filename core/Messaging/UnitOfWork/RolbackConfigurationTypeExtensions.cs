using System;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public static class RolbackConfigurationTypeExtensions
    {
        public static bool RollBackOn(this RollbackConfigurationType value, Exception exception)
        {
            switch (value)
            {
                case RollbackConfigurationType.Never:
                    return false;
                case RollbackConfigurationType.AnyThrowable:
                    return true;
                case RollbackConfigurationType.UncheckedException:
                    return true; // TODO: confirm mapping of exceptions to .net types
                case RollbackConfigurationType.RuntimeExceptions:
                    return true;
                default:
                    return true;
            }
        }
    }
}